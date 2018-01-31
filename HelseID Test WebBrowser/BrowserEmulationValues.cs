namespace HelseID.Clients.WPF.EmbeddedBrowser
{
    internal static class BrowserEmulationValues
    {
        public static uint Edge = 0x2AF9;
        public static uint Ie11 = 0x2AF8;
        public static uint Ie10Fixed = 0x2711;
        public static uint Ie10 = 0x02710;
        public static uint Ie9Fixed = 0x270F;
        public static uint Ie9 = 0x2328;
        public static uint Ie8Fixed = 0x22B8;
        public static uint Ie8 = 0x1F40;
        public static uint Ie7 = 0x1B58;

        public static uint BrowserVersion(int version)
        {
            switch (version)
            {
                case 7:
                    return Ie7;                    
                case 8:
                    return Ie8;
                case 9:
                    return Ie9;
                case 10:
                    return Ie10;
                case 11:
                    return Ie11;
                default:
                    return Ie11;
            }
        }
    }
}
