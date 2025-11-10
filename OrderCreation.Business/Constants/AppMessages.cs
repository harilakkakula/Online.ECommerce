namespace OrderCreation.Business.Constants
{
    public static class AppMessages
    {
        #region General Messages

        public const string NotFound = "{0} with ID '{1}' was not found.";
        public const string InvalidRequest = "The request object cannot be null or empty.";
        public const string ValidationFailed = "Validation failed for the provided request: {0}.";
        public const string UnexpectedError = "An unexpected error occurred during the operation.";
        public const string UnexpectedErrorCreatingUser = "An unexpected error occurred while creating a user.";
        public const string UnexpectedErrorCreatingOrder = "An unexpected error occurred while creating an order.";
        public const string ErrorRetrievingUsers = "An error occurred while retrieving users.";
        public const string ErrorRetrievingOrders = "An error occurred while retrieving orders.";
        public const string ErrorGettingUserById = "An error occurred while retrieving user with ID {0}.";
        public const string ErrorGettingOrderById = "An error occurred while retrieving order with ID {0}.";
        public const string InvalidId = "Invalid Id.";

        #endregion

        #region User Messages

        public const string UserAlreadyExists = "A user with the email '{0}' already exists.";
        public const string UserCreatedSuccess = "User created successfully with ID: {0}.";
        public const string UserNotFound = "User not found with ID: {0}.";
        public const string UserEventPublished = "User created event published to Kafka topic: {0}.";
        public const string UsersRetrievedSuccess = "Users retrieved successfully.";
        public const string UserRetrievedSuccess = "User retrieved successfully with ID: {0}.";

        #endregion

        #region Order Messages

        public const string OrderCreatedSuccess = "Order created successfully with ID: {0}.";
        public const string OrderNotFound = "Order not found with ID: {0}.";
        public const string OrderEventPublished = "Order created event published to Kafka topic: {0}.";
        public const string OrdersRetrievedSuccess = "Orders retrieved successfully.";
        public const string OrderRetrievedSuccess = "Order retrieved successfully with ID: {0}.";
        public const string InvalidOrderRequest = "The order request cannot be null or empty.";
        public const string OrderValidationFailed = "Order validation failed.";
        public const string OrderValidationFailedWithDetails = "Order validation failed: {0}.";

        #endregion

        #region Event Handling Messages

        public const string EmptyEventMessage = "Received an empty message for topic: {0}.";
        public const string InvalidEventPayload = "Received an invalid or null payload for topic: {0}.";
        public const string EventDeserializationError = "Failed to deserialize the event message for topic: {0}.";
        public const string EventProcessingStarted = "Started processing event with ID: {0}, Name: {1}, Topic: {2}.";
        public const string EventProcessingSuccess = "Successfully processed event with ID: {0} from topic: {1}.";
        public const string EventProcessingError = "An unexpected error occurred while processing the event for topic: {0}.";

        #endregion
    }
}
