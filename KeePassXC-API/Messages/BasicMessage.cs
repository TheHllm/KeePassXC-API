namespace KeePassXC_API.Messages
{
    class BasicMessage : Message
    {
        public BasicMessage(Actions type) : base()
        {
            Action = type;
        }
    }
}
