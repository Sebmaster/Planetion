using System;

namespace Planetion.Utils {
    public class RequireComponent : Attribute {

        public Type[] Types;

        public RequireComponent(params Type[] t) {
            Types = t;
        }
    }
}
