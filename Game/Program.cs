using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Platform;
using Planetion;
using Planetion.Engine.Objects;
using Planetion.Engine.Utils;
using Planetion.Internals;
using Planetion.Loaders;
using Planetion.Objects;
using System.Drawing;
using Planetion.Engine;
using Planetion.Game.Objects;

namespace Demo {

    public class Program {

        protected static GameSurface game;

        GameObject tank;
        protected static void OnLoad(object sender, EventArgs e) {
            Mesh m = ObjLoader.Load("robo1.obj");
            Mesh m2 = ObjLoader.Load("box.obj");
            //Mesh m = new BoxMesh();
            /*
            tank = World.Instantiate();
            tank.Transform.LocalPosition = new OpenTK.Vector3(100, 0, 0);
            //tank.Transform.Rotate(OpenTK.Vector3.UnitX, 90);
            //obj.Transform.Scale = new OpenTK.Vector3(1/100f);
            tank.AddComponent<MeshComponent>().MeshObject = m;

            GameObject obj2 = World.Instantiate();
            obj2.Transform.Parent = tank.Transform;
            obj2.Transform.Rotate(OpenTK.Vector3.UnitY, 90);
            obj2.Transform.LocalPosition = new OpenTK.Vector3(-100, 0, 0);
            obj2.AddComponent<MeshComponent>().MeshObject = m;
            //RigidBody b = new Jitter.Dynamics.RigidBody(new Jitter.Collision.Shapes.BoxShape(1, 1, 1));

            //obj.AddComponent<RigidbodyComponent>().Body = b;
            */
            /*
            GameObject terrain = World.Instantiate();
            TerrainComponent tc = terrain.AddComponent<TerrainComponent>();
            tc.HeightMap = new float[2, 2];
            tc.HeightMap[0, 0] = 0;
            tc.HeightMap[1, 0] = 0;
            tc.HeightMap[0, 1] = 0;
            tc.HeightMap[1, 1] = 0;
            tc.Regenerate();
            terrain.Transform.Scale = new Vector3(200, 200, 200);
            */
            GameObject obj = game.World.Instantiate();
            //obj.Transform.Parent = tank.Transform;
            //obj.Transform.Position = new OpenTK.Vector3(250, 0, 250);
            //obj.Transform.Scale = new OpenTK.Vector3(20, 20, 20);
            //obj.AddComponent<MeshComponent>().MeshObject = new BoxMesh();
            TerrainComponent tc = obj.AddComponent<TerrainComponent>();

            tc.HeightMap = new float[,] { 
                {0,0,0, 0},
                {0,3,3,0},
                {0,3,3,0},
                {0,0,0,0}
            };

            tc.Texture = Bitmap.FromFile("terraintexture.jpg") as Bitmap;
            
            Heightmap map = new Heightmap(256);
            map.SetNoise(5, 8);
            map.Erode(0);
            map.Smoothen();
            map.Smoothen();
            map.Perturb(0.01f, 0.3f);
            tc.HeightMap = new float[map.Size, map.Size];

            for (int i = 0; i < tc.HeightMap.GetLength(0); i++) {
                for (int j = 0; j < tc.HeightMap.GetLength(1); j++) {
                    //tc.HeightMap[i, j] = (float)Random.NextDouble();
                    //tc.HeightMap[i, j] = Perlin.Noise2(i/40f, j/40f);
                    //tc.HeightMap[i, j] = -5 + j+i;
                    tc.HeightMap[i, j] = map[i, j] * 5 ;
                    //tc2.HeightMap[i, j] = map2[i, j];
                }
            }

            tc.HeightMap[110, 110] = 20;
            tc.HeightMap[128, 128] = 2;
            tc.HeightMap[129, 128] = 2;
            tc.HeightMap[128, 129] = 2;
            tc.HeightMap[129, 129] = 2;
            tc.HeightMap[130, 128] = 2;
            tc.HeightMap[131, 128] = 2;
            tc.HeightMap[130, 129] = 2;
            tc.HeightMap[131, 129] = 2;
            /*tc.HeightMap = new float[3, 3];
            tc.HeightMap[0, 0] = 1;
            tc.HeightMap[1, 0] = 1;
            tc.HeightMap[0, 1] = 1;
            tc.HeightMap[1, 1] = 0;
            
             */
            /*
            tc.HeightMap[50, 50] = float.NaN;
            tc.Disabled[50, 52] = true;*/
            tc.Regenerate();

            //BillboardComponent bill = obj.AddComponent<BillboardComponent>();
            //bill.Create("M1_ABRAM.BMP");
            //bill.RotationMode = BillboardComponent.RotationModes.FACE_CAMERA;

            //obj.Transform.Rotate(OpenTK.Vector3.UnitY, 180);
            //obj.Transform.Scale = new OpenTK.Vector3(70,70,70);

            GameObject camObj = game.World.Instantiate();
            camObj.Transform.LocalPosition = new OpenTK.Vector3(0, 0, 0);
            camObj.AddComponent<FPSCamera>();
            CameraComponent cam = camObj.GetComponent<CameraComponent>();
            cam.Mode = CameraComponent.Rendering.DEFERRED;
            cam.LookAt(new OpenTK.Vector3(0, 0, 0));


            GameObject light = game.World.Instantiate();
            DirectionalLight s = light.AddComponent<DirectionalLight>();
            light.AddComponent<TestComponent>();
            s.Intensity = 5;
            s.Color = Color4.Blue;
            s.CastShadows = true;
            light.Transform.Position = new Vector3(500, 200, 0);
            light.Transform.Rotate(Vector3.UnitZ, -100);
            light.Transform.Rotate(Vector3.UnitY, 45);
            light.Transform.Scale = new Vector3(10, 10, 10);
            //camObj.Children.Add(light);
            /*
            light = World.Instantiate();
            s = light.AddComponent<DirectionalLight>();
            light.AddComponent<TestComponent>();
            s.Intensity = 0.5f;
            s.Color = Color4.Green;
            s.CastShadows = true;
            light.Transform.Position = new Vector3(500, 200, 0);
            light.Transform.Rotate(Vector3.UnitZ, -150);
            light.Transform.Rotate(Vector3.UnitY, 47);
            light.AddComponent<MeshComponent>().MeshObject = m2;
            light.Transform.Scale = new Vector3(10, 10, 10);
            */
            
            for (int i = 0; i < 1; i++) {
                for (int j = 0; j < 1; j++) {
                    for (int k = 0; k < 1; k++) {
                        GameObject t = game.World.Instantiate();
                        t.AddComponent<MeshComponent>().MeshObject = m;
                        t.Transform.Rotate(Vector3.UnitY, 90);
                        t.Transform.LocalPosition = new OpenTK.Vector3(i * 180, j * 100, k * 180);
                        t.Transform.Scale = new OpenTK.Vector3(1, 1, 1);
                        //t.Transform.Rotation = new OpenTK.Quaternion(new Vector3((float)(Random.NextDouble() * 2 - 1), (float)(Random.NextDouble() * 2 - 1), (float)(Random.NextDouble() * 2 - 1)), 0);
                        t.AddComponent<TestComponent2>();
                    }
                }
            }
            
            //camObj.AddComponent<Movement>();


            /*
            for (int i=0; i < 99; ++i) {
                obj = new GameObject();
                obj.Transform.Position.X+=0;
                obj.AddComponent<MeshComponent>().MeshObject = m;
                //obj.AddComponent<Movement>();
                lst.Add(obj);
            }*/

            /*
            obj = new GameObject();
            obj.Transform.Position = new OpenTK.Vector3(0, 100, 100);
            //obj.AddComponent<LightEmitterComponent>();
            lst.Add(obj);
             */
            /*
             obj = new GameObject();
            BillboardComponent bill = obj.AddComponent<BillboardComponent>();
            bill.Create("M1_ABRAM.BMP");
            bill.RotationMode = BillboardComponent.RotationModes.FACE_CAMERA;

            obj.Transform.Scale = new OpenTK.Vector3(70,70,70);
            lst.Add(obj);*/

        }

        public static void Main(string[] args) {
            game = new GameSurface(new OpenTK.GameWindow(800, 600, new GraphicsMode(new ColorFormat(32), 24, 0, 0), "Testgame", GameWindowFlags.Default, DisplayDevice.GetDisplay(DisplayIndex.Default), 3, 2, 
#if DEBUG
                GraphicsContextFlags.Debug
#else
                GraphicsContextFlags.ForwardCompatible
#endif
));
            game.Window.Load += OnLoad;
            game.Start();
        }
    }
}
