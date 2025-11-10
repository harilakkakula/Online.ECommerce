namespace UserCreation.Business.Constants
{
    public static class AppMessages
    {

        #region General
        public const string UnexpectedError = "An unexpected error occurred.";
        public const string ValidationFailed = "Validation failed for the provided data.";
        public const string NotFound = "{0} with ID {1} not found.";
        public const string InvalidRequest = "The request payload is invalid.";
        public const string InvalidId = "Invalid Id.";
        #endregion

        #region Event Handling Messages

        public const string EmptyEventMessage = "Received an empty message for topic: {0}.";
        public const string InvalidEventPayload = "Received an invalid or null payload for topic: {0}.";
        public const string EventDeserializationError = "Failed to deserialize the event message for topic: {0}.";
        public const string EventProcessingStarted = "Started processing event with ID: {0}, Name/Product: {1}, Topic: {2}.";
        public const string EventProcessingSuccess = "Successfully processed event with ID: {0} from topic: {1}.";
        public const string EventProcessingError = "An unexpected error occurred while processing the event for topic: {0}.";

        #endregion

        #region Kafka / Messaging
        public const string KafkaWaiting = "Waiting for Kafka...";
        public const string KafkaProducedMessage = "Produced message to {0}: {1}";
        public const string KafkaTopicConfigMissing = "Kafka topic configuration missing. Default topic used.";
        #endregion

        #region User - Business Service
        public const string UserCreatedSuccess = "User {UserId} created successfully.";
        public const string UserEventPublished = "User created event published to Kafka topic '{Topic}'.";
        public const string UserNotFound = "User with ID {UserId} not found.";
        public const string ValidationError = "Validation error while creating user: {Message}";
        public const string UnexpectedErrorCreatingUser = "Unexpected error while creating user.";
        public const string ErrorRetrievingUsers = "Error retrieving users.";
        public const string ErrorGettingUserById = "Error getting user by ID {UserId}.";
        public const string UserAlreadyExists = "A user with the email '{0}' already exists.";

        #endregion

        #region User - Controller Responses
        public const string ErrorCreatingUserResponse = "An error occurred while creating the user.";
        public const string FailedToRetrieveUsersResponse = "Failed to retrieve users.";
        public const string FailedToRetrieveUserResponse = "Failed to retrieve the requested user.";
        public const string UserCreatedResponse = "User created successfully.";
        #endregion

        #region Order - Order Service
        public const string UnexpectedErrorCreatingOrder = "An unexpected error occurred while creating an order.";
        public const string OrderCreatedSuccess = "Order created successfully with ID: {0}.";
        public const string OrderNotFound = "Order not found with ID: {0}.";
        public const string ErrorGettingOrderById = "An error occurred while retrieving order with ID {0}.";
        public const string OrderAlreadyExists = "An order with the same details already exists.";
        #endregion
    }
}
