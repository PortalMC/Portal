namespace Portal.Utils
{
    public class AccessRightLevel
    {
        public static readonly AccessRightLevel Owner = new AccessRightLevel(0);

        public int Level { get; }

        private AccessRightLevel(int level)
        {
            Level = level;
        }
    }
}