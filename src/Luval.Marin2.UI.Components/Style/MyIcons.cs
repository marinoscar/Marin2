using Microsoft.FluentUI.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luval.Marin2.UI.Components.Style
{
    public static class MyIcons
    {
        public class Google : Icon { public Google() : base("GoogleLogo", IconVariant.Regular, IconSize.Size20, "<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 488 512\"><!--!Font Awesome Free 6.7.1 by @fontawesome - https://fontawesome.com License - https://fontawesome.com/license/free Copyright 2024 Fonticons, Inc.--><path d=\"M488 261.8C488 403.3 391.1 504 248 504 110.8 504 0 393.2 0 256S110.8 8 248 8c66.8 0 123 24.5 166.3 64.9l-67.5 64.9C258.5 52.6 94.3 116.6 94.3 256c0 86.5 69.1 156.6 153.7 156.6 98.2 0 135-70.4 140.8-106.9H248v-85.3h236.1c2.3 12.7 3.9 24.9 3.9 41.4z\"/></svg>") { } }

        public static class Regular
        {
            public class  Home : Icon
            {
                public Home() :base("Home", IconVariant.Regular, IconSize.Size20, "<svg width=\"24\" height=\"24\" fill=\"none\" viewBox=\"0 0 24 24\" xmlns=\"http://www.w3.org/2000/svg\"><path d=\"M10.55 2.533a2.25 2.25 0 0 1 2.9 0l6.75 5.695c.508.427.8 1.056.8 1.72v9.802a1.75 1.75 0 0 1-1.75 1.75h-3a1.75 1.75 0 0 1-1.75-1.75v-5a.75.75 0 0 0-.75-.75h-3.5a.75.75 0 0 0-.75.75v5a1.75 1.75 0 0 1-1.75 1.75h-3A1.75 1.75 0 0 1 3 19.75V9.947c0-.663.292-1.292.8-1.72l6.75-5.694Z\" fill=\"#ffffff\"/></svg>") { }
            }
        }

        public static class Filled
        {
            public class Home : Icon
            {
                public Home() : base("Home", IconVariant.Filled, IconSize.Size20, "<svg width=\"24\" height=\"24\" fill=\"none\" viewBox=\"0 0 24 24\" xmlns=\"http://www.w3.org/2000/svg\"><path d=\"M10.55 2.533a2.25 2.25 0 0 1 2.9 0l6.75 5.695c.508.427.8 1.056.8 1.72v9.802a1.75 1.75 0 0 1-1.75 1.75h-3a1.75 1.75 0 0 1-1.75-1.75v-5a.75.75 0 0 0-.75-.75h-3.5a.75.75 0 0 0-.75.75v5a1.75 1.75 0 0 1-1.75 1.75h-3A1.75 1.75 0 0 1 3 19.75V9.947c0-.663.292-1.292.8-1.72l6.75-5.694Z\" fill=\"#ffffff\"/></svg>") { }
            }
        }

    }
}
