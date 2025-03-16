namespace NSSM.Core.Messaging
{
    /// <summary>
    /// Implementation of the message bus that handles message publication and subscription.
    /// This enables loosely coupled communication between different components of the application.
    /// </summary>
    public class MessageBus : IMessageBus
    {
        private readonly Dictionary<Type, List<object>> _subscriptions = new();
        private readonly object _lockObject = new();

        /// <summary>
        /// Publishes a message to all subscribers of the specified message type.
        /// </summary>
        /// <typeparam name="TMessage">The type of message to publish</typeparam>
        /// <param name="message">The message instance to publish</param>
        public void Publish<TMessage>( TMessage message ) where TMessage : class
        {
            if( message == null )
            {
                throw new ArgumentNullException( nameof( message ) );
            }

            Type messageType = typeof( TMessage );
            List<object>? subscribers = null;

            //get subs for this message type
            lock( _lockObject )
            {
                if( _subscriptions.TryGetValue( messageType, out var subscribersList ) )
                {
                    subscribers = subscribersList.ToList(); //create a copy to avoid modification during enumeration
                }
            }

            //notify all subs
            if( subscribers != null )
            {
                foreach( var subscription in subscribers )
                {
                    if( subscription is Action<TMessage> action )
                    {
                        try
                        {
                            action( message );
                        }
                        catch( Exception )
                        {
                            //swallow exceptions in subscribers to prevent cascading failures
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Subscribes to messages of the specified type.
        /// </summary>
        /// <typeparam name="TMessage">The type of message to subscribe to</typeparam>
        /// <param name="action">The action to execute when a message is received</param>
        /// <returns>A subscription token that can be used to unsubscribe</returns>
        public IDisposable Subscribe<TMessage>( Action<TMessage> action ) where TMessage : class
        {
            if( action == null )
            {
                throw new ArgumentNullException( nameof( action ) );
            }

            Type messageType = typeof( TMessage );

            lock( _lockObject )
            {
                if( !_subscriptions.TryGetValue( messageType, out var subscribers ) )
                {
                    subscribers = new List<object>();
                    _subscriptions[messageType] = subscribers;
                }

                subscribers.Add( action );
            }

            //return a disposable that can be used to unsubscribe
            return new SubscriptionToken( () => Unsubscribe( messageType, action ) );
        }

        /// <summary>
        /// Unsubscribes from messages of the specified type.
        /// </summary>
        /// <param name="messageType">The type of message to unsubscribe from</param>
        /// <param name="subscriber">The subscriber to remove</param>
        private void Unsubscribe( Type messageType, object subscriber )
        {
            lock( _lockObject )
            {
                if( _subscriptions.TryGetValue( messageType, out var subscribers ) )
                {
                    subscribers.Remove( subscriber );

                    if( subscribers.Count == 0 )
                    {
                        _subscriptions.Remove( messageType );
                    }
                }
            }
        }

        /// <summary>
        /// Token that allows unsubscribing from messages.
        /// </summary>
        private class SubscriptionToken : IDisposable
        {
            private readonly Action _unsubscribeAction;
            private bool _disposed;

            public SubscriptionToken( Action unsubscribeAction )
            {
                _unsubscribeAction = unsubscribeAction ?? throw new ArgumentNullException( nameof( unsubscribeAction ) );
            }

            public void Dispose()
            {
                if( !_disposed )
                {
                    _unsubscribeAction();
                    _disposed = true;
                }
            }
        }
    }
}