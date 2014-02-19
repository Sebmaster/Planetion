using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace Planetion.Engine.Utils {
    public class Frustum {
        public Vector3[] Points {
            get;
            set;
        }
        public float Near {
            get;
            set;
        }
        public float Far {
            get;
            set;
        }
        public float Angle {
            get;
            set;
        }
        public float AspectRatio {
            get;
            set;
        }

        private float tang;
        private float nw, nh, fw, fh;

        public Frustum() {
            Points = new Vector3[8];
        }
        public void SetCamInternals(float angle, float ratio, float nearD, float farD) {
            // compute width and height of the near and far plane sections

            Angle = angle;
            AspectRatio = ratio;
            Near = nearD;
            Far = farD;
            
            tang = (float)Math.Tan(MathHelper.DegreesToRadians(angle * 0.5f));
            nh = nearD * tang;
            nw = nh * ratio;
            fh = farD * tang;
            fw = fh * ratio;
        }

        public void SetCamDef(Vector3 eye, Vector3 postion, Vector3 up) {
            Vector3 nc, fc, X, Y, Z;

            // compute the Z axis of camera
            // this axis points in the opposite direction from
            // the looking direction
            Z = eye - postion;
            Z.Normalize();

            // X axis of camera with given "up" vector and Z axis
            X = Vector3.Cross(up, Z);
            X.Normalize();

            // the real "up" vector is the cross product of Z and X
            Y = Vector3.Cross(Z, X);

            // compute the centers of the near and far planes
            nc = eye - Z * Near;
            fc = eye - Z * Far;

            // compute the 4 corners of the frustum on the near plane
            Points[0] = nc + Y * nh - X * nw;
            Points[1] = nc + Y * nh + X * nw;
            Points[2] = nc - Y * nh - X * nw;
            Points[3] = nc - Y * nh + X * nw;

            // compute the 4 corners of the frustum on the far plane
            Points[4] = fc + Y * fh - X * fw;
            Points[5] = fc + Y * fh + X * fw;
            Points[6] = fc - Y * fh - X * fw;
            Points[7] = fc - Y * fh + X * fw;
        }
    }
}
