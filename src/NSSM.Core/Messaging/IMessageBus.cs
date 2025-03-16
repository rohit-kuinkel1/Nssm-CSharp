namespace NSSM.Core.Messaging
{
    /// <summary>
    /// Defines the contract for a message bus that enables loosely coupled communication between components.
    /// Components can publish and subscribe to messages without having direct dependencies on each other.
    /// </summary>
    public interface IMessageBus
    {
        /// <summary>
        /// Publishes a message to all subscribers of the specified message type.
        /// </summary>
        /// <typeparam name="TMessage">The type of message to publish</typeparam>
        /// <param name="message">The message instance to publish</param>
        void Publish<TMessage>( TMessage message ) where TMessage : class;

        /// <summary>
        /// Subscribes to messages of the specified type.
        /// </summary>
        /// <typeparam name="TMessage">The type of message to subscribe to</typeparam>
        /// <param name="action">The action to execute when a message is received</param>
        /// <returns>A subscription token that can be used to unsubscribe</returns>
        IDisposable Subscribe<TMessage>( Action<TMessage> action ) where TMessage : class;
    }
}