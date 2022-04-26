using OpenTK.Windowing.Desktop;
using System;
using System.Collections.Generic;
using LearnOpenTK.Common;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Mathematics;


namespace Project
{
    static class Constants
    {
        public const string path = "../../../Shaders/";
    }
    internal class Window : GameWindow
    {
        List<Asset3d> _objects3d = new List<Asset3d>();
        double _time;
        Camera _camera;
        bool _firstMove = true;
        Vector2 _lastPos;
        Vector3 _objectPos = new Vector3(0, 0, 0);

        float _rotationSpeed = 0.5f;
        float wavingSpeed = 0.2f;
        float jumpSpeed = 2.3f;
        float winkSpeed = 0.2f;

        public Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings)
        {
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            //background
            GL.ClearColor(0.576f, 0.858f, 0.964f, 1);
            GL.Enable(EnableCap.DepthTest);

            loadBaymax();
            loadOlaf();
            loadEve();
            loadAlas();
            loadSnow(120);
            loadCloud();
            loadTrees();
            loadRocks();
            loadMountains();
            loadFlowers();

            foreach (Asset3d _object3d in _objects3d)
                _object3d.load(Constants.path + "shader.vert", Constants.path + "shader.frag", Size.X, Size.Y);

            initializeCamera();
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            //baymax
            jump(_objects3d[0], 0.3f);
            wink(_objects3d[0].Child[0], _objects3d[0].Child[1], 0.14f);

            //olaf
            jump2(_objects3d[1], 0.2f);
            selfRotate2(_objects3d[1]);

            //eve
            jump(_objects3d[2].Child[0], 0.1f);
            waveEveHands();

            snowFlow(new Vector3(0, 0, 0));
            cloudRotate();
            selfRotate(_objects3d[9].Child[0]);
            selfRotate(_objects3d[9].Child[1]);

            foreach (Asset3d _object3d in _objects3d)
                _object3d.render(3, _time, _camera.GetViewMatrix(), _camera.GetProjectionMatrix());

            SwapBuffers();
        }

        private void jump(Asset3d obj, float jumpHeight)
        {
            float minHeight = obj.defaultPosition.Y;
            float maxHeight = obj.defaultPosition.Y + jumpHeight;
            float posY = obj._centerPosition.Y;

            if (posY > maxHeight || posY < minHeight)
                jumpSpeed *= -1;

            obj.createTranslation(new Vector3(0, 0.001f * jumpSpeed, 0));
        }

        private void jump2(Asset3d obj, float jumpHeight)
        {
            float minHeight = obj.defaultPosition.Y;
            float maxHeight = obj.defaultPosition.Y + jumpHeight;
            float posY = obj._centerPosition.Y;

            if (posY > maxHeight || posY < minHeight)
                jumpSpeed *= -1;

            obj.createTranslation(new Vector3(0, 0.001f * jumpSpeed, 0));
        }

        private void wink(Asset3d obj1, Asset3d obj2, float minZ)
        {
            float maxZ = obj1.defaultPosition.Z;
            float posZ = obj1._centerPosition.Z;

            if (posZ > maxZ || posZ < minZ)
                winkSpeed *= -1;

            obj1.createTranslation(new Vector3(0, 0, 0.001f * winkSpeed));
            obj2.createTranslation(new Vector3(0, 0, 0.001f * winkSpeed));
        }

        private void waveEveHands()
        {
            Asset3d patokan = _objects3d[2].Child[3];
            Asset3d center = _objects3d[2].Child[1];

            float minHeight = 0.36f;
            float maxHeight = 0.36f;

            if (patokan._centerPosition.Y - center._centerPosition.Y > maxHeight || center._centerPosition.Y - patokan._centerPosition.Y > minHeight)
                wavingSpeed *= -1;

            _objects3d[2].Child[2].rotatede(center._centerPosition, Vector3.UnitZ, -wavingSpeed);
            _objects3d[2].Child[3].rotatede(center._centerPosition, Vector3.UnitZ, wavingSpeed);
            patokan.rotatede(center._centerPosition, Vector3.UnitZ, wavingSpeed);
        }

        private void selfRotate(Asset3d obj)
        {
            obj.rotatede(obj._centerPosition, obj._euler[1], 2f);
        }

        public void selfRotate2(Asset3d obj)
        {
            Vector3 pivot = new Vector3(_objects3d[0]._centerPosition.Y - 1.15f, 0, 0);
            obj.rotatede(pivot, obj._euler[1], -0.15f);
        }

        private void cloudRotate()
        {
            _objects3d[5].Child[0].rotatede(new Vector3(0, 0, 0), Vector3.UnitY, 0.1f);
            _objects3d[5].Child[1].rotatede(new Vector3(0, 0, 0), Vector3.UnitY, -0.1f);
        }

        private void snowFlow(Vector3 pivot)
        {
            _objects3d[4].rotatede(pivot, _objects3d[0]._euler[1], -0.5f);
            _objects3d[4].rotatede(pivot, _objects3d[0]._euler[2], 0.5f);
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0, 0, Size.X, Size.Y);
            _camera.AspectRatio = Size.X / (float)Size.Y;
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);
            //_camera.Fov = _camera.Fov - e.OffsetY;
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);

            if (KeyboardState.IsKeyDown(Keys.Escape))
                Close();

            float cameraSpeed = 1f;

            if (KeyboardState.IsKeyDown(Keys.W))
                _camera.Position += _camera.Front * cameraSpeed * (float)args.Time;

            if (KeyboardState.IsKeyDown(Keys.A))
                _camera.Position -= _camera.Right * cameraSpeed * (float)args.Time;
            
            if (KeyboardState.IsKeyDown(Keys.D))
                _camera.Position += _camera.Right * cameraSpeed * (float)args.Time;
            
            if (KeyboardState.IsKeyDown(Keys.S))
                _camera.Position -= _camera.Front * cameraSpeed * (float)args.Time;
            
            if (KeyboardState.IsKeyDown(Keys.Up))
                _camera.Position += _camera.Up * cameraSpeed * (float)args.Time;
            
            if (KeyboardState.IsKeyDown(Keys.Down))
                _camera.Position -= _camera.Up * cameraSpeed * (float)args.Time;

            if (KeyboardState.IsKeyDown(Keys.Left))
                _camera.Yaw -= cameraSpeed * (float)args.Time * 60;

            if (KeyboardState.IsKeyDown(Keys.Right))
                _camera.Yaw += cameraSpeed * (float)args.Time * 60;

            var mouse = MouseState;
            var senstivity = 0.1f;

            if (_firstMove)
            {
                _lastPos = new Vector2(mouse.X, mouse.Y);
                _firstMove = false;
            }
            else
            {
                var deltaX = mouse.X - _lastPos.X;
                var deltaY = mouse.Y - _lastPos.Y;
                _lastPos = new Vector2(mouse.X, mouse.Y);
                _camera.Yaw += deltaX * senstivity;
                _camera.Pitch -= deltaY * senstivity;
            }

            if (KeyboardState.IsKeyDown(Keys.M))
            {
                var axis = new Vector3(0, 1, 0);
                _camera.Position -= _objectPos;
                _camera.Yaw -= _rotationSpeed;
                _camera.Position = Vector3.Transform(_camera.Position, generateArbRotationMatrix(axis, _objectPos, -_rotationSpeed).ExtractRotation());
                _camera.Position += _objectPos;
                _camera._front = -Vector3.Normalize(_camera.Position - _objectPos);
            }

            if (KeyboardState.IsKeyDown(Keys.R))
            {
                initializeCamera();
            }
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButton.Left)
            {
                float _x = (MousePosition.X - Size.X / 2) / (Size.X / 2);
                float _y = -(MousePosition.Y - Size.Y / 2) / (Size.Y / 2);

                Console.WriteLine("x = " + _x + "y = " + _y);
            }
        }

        public Matrix4 generateArbRotationMatrix(Vector3 axis, Vector3 center, float degree)
        {
            var rads = MathHelper.DegreesToRadians(degree);

            var secretFormula = new float[4, 4] {
                { (float)Math.Cos(rads) + (float)Math.Pow(axis.X, 2) * (1 - (float)Math.Cos(rads)), axis.X* axis.Y * (1 - (float)Math.Cos(rads)) - axis.Z * (float)Math.Sin(rads),    axis.X * axis.Z * (1 - (float)Math.Cos(rads)) + axis.Y * (float)Math.Sin(rads),   0 },
                { axis.Y * axis.X * (1 - (float)Math.Cos(rads)) + axis.Z * (float)Math.Sin(rads),   (float)Math.Cos(rads) + (float)Math.Pow(axis.Y, 2) * (1 - (float)Math.Cos(rads)), axis.Y * axis.Z * (1 - (float)Math.Cos(rads)) - axis.X * (float)Math.Sin(rads),   0 },
                { axis.Z * axis.X * (1 - (float)Math.Cos(rads)) - axis.Y * (float)Math.Sin(rads),   axis.Z * axis.Y * (1 - (float)Math.Cos(rads)) + axis.X * (float)Math.Sin(rads),   (float)Math.Cos(rads) + (float)Math.Pow(axis.Z, 2) * (1 - (float)Math.Cos(rads)), 0 },
                { 0, 0, 0, 1}
            };
            var secretFormulaMatix = new Matrix4
            (
                new Vector4(secretFormula[0, 0], secretFormula[0, 1], secretFormula[0, 2], secretFormula[0, 3]),
                new Vector4(secretFormula[1, 0], secretFormula[1, 1], secretFormula[1, 2], secretFormula[1, 3]),
                new Vector4(secretFormula[2, 0], secretFormula[2, 1], secretFormula[2, 2], secretFormula[2, 3]),
                new Vector4(secretFormula[3, 0], secretFormula[3, 1], secretFormula[3, 2], secretFormula[3, 3])
            );

            return secretFormulaMatix;
        }

        private void initializeCamera()
        {
            _camera = new Camera(new Vector3(0, 0.35f, 3.5f), Size.X / Size.Y);
        }

        private void loadBaymax()
        {
            //kepala
            var baymax = new Asset3d(new Vector3(0.8f, 0.8f, 0.8f));
            baymax.createEllipsoid2(0.2f, 0.1f, 0.18f, 0, 0.88f, 0, 100, 100);
            baymax.defaultPosition = new Vector3(0, 0.88f, 0);

                //mata
                var mataKiri = new Asset3d(new Vector3(0, 0, 0));
                var mataKanan = new Asset3d(new Vector3(0, 0, 0));
                mataKiri.createEllipsoid2(0.02f, 0.02f, 0.02f, -0.08f, 0.86f, 0.16f, 100, 100);
                mataKanan.createEllipsoid2(0.02f, 0.02f, 0.02f, 0.08f, 0.86f, 0.16f, 100, 100);
                mataKiri.defaultPosition = new Vector3(-0.08f, 0.86f, 0.16f);
                mataKanan.defaultPosition = new Vector3(0.08f, 0.86f, 0.16f);

            //garis mata
            var garisMata = new Asset3d(new Vector3(0, 0, 0));
                garisMata.createCuboid(0.15f, 0.008f, 0.02f, 0, 0.86f, 0.17f);

            // badan
            var badanAtas = new Asset3d(new Vector3(0.9f, 0.9f, 0.9f));
            var badanBawah = new Asset3d(new Vector3(0.9f, 0.9f, 0.9f));
            badanAtas.createEllipsoid2(0.45f, 0.5f, 0.4f, 0, 0.1f, 0, 100, 100);
            badanBawah.createEllipsoid2(0.4f, 0.5f, 0.35f, 0, 0.3f, 0, 100, 100);

            //tangan
            var tanganKiri = new Asset3d(new Vector3(0.8f, 0.8f, 0.8f));
            var tanganKanan = new Asset3d(new Vector3(0.8f, 0.8f, 0.8f));
            tanganKiri.createEllipsoid2(0.14f, 0.5f, 0.13f, -0.45f, 0.28f, 0, 100, 100);
            tanganKanan.createEllipsoid2(0.14f, 0.5f, 0.13f, 0.45f, 0.28f, 0, 100, 100);
            tanganKiri.rotatede(tanganKiri._centerPosition, tanganKiri._euler[2], -20);
            tanganKanan.rotatede(tanganKanan._centerPosition, tanganKanan._euler[2], 20);

                //JariKiri
                var jariKiri1 = new Asset3d(new Vector3(0.9f, 0.9f, 0.9f));
                var jariKiri2 = new Asset3d(new Vector3(0.9f, 0.9f, 0.9f));
                var jariKiri3 = new Asset3d(new Vector3(0.9f, 0.9f, 0.9f));
                var jariKiri4 = new Asset3d(new Vector3(0.9f, 0.9f, 0.9f));
                jariKiri1.createEllipsoid2(0.03f, 0.045f, 0.04f, -0.5f, -0.15f, 0, 100, 100);
                jariKiri2.createEllipsoid2(0.03f, 0.09f, 0.01f, -0.6f, -0.2f, 0, 100, 100);
                jariKiri3.createEllipsoid2(0.03f, 0.09f, 0.01f, -0.58f, -0.2f, -0.03f, 100, 100);
                jariKiri4.createEllipsoid2(0.03f, 0.09f, 0.01f, -0.58f, -0.2f, -0.05f, 100, 100);
                jariKiri1.rotatede(jariKiri1._centerPosition, jariKiri1._euler[2], 10);
                jariKiri2.rotatede(jariKiri2._centerPosition, jariKiri2._euler[2], 30);
                jariKiri3.rotatede(jariKiri3._centerPosition, jariKiri3._euler[2], 30);
                jariKiri4.rotatede(jariKiri4._centerPosition, jariKiri4._euler[2], 30);

                //JariKanan
                var jariKanan1 = new Asset3d(new Vector3(0.9f, 0.9f, 0.9f));
                var jariKanan2 = new Asset3d(new Vector3(0.9f, 0.9f, 0.9f));
                var jariKanan3 = new Asset3d(new Vector3(0.9f, 0.9f, 0.9f));
                var jariKanan4 = new Asset3d(new Vector3(0.9f, 0.9f, 0.9f));
                jariKanan1.createEllipsoid2(0.03f, 0.045f, 0.04f, 0.5f, -0.15f, 0, 100, 100);
                jariKanan2.createEllipsoid2(0.03f, 0.09f, 0.04f, 0.6f, -0.2f, 0f, 100, 100);
                jariKanan3.createEllipsoid2(0.03f, 0.09f, 0.01f, 0.58f, -0.2f, -0.03f, 100, 100);
                jariKanan4.createEllipsoid2(0.03f, 0.09f, 0.01f, 0.58f, -0.2f, -0.05f, 100, 100);
                jariKanan1.rotatede(jariKanan1._centerPosition, jariKanan1._euler[2], -10);
                jariKanan2.rotatede(jariKanan2._centerPosition, jariKanan2._euler[2], -30);
                jariKanan3.rotatede(jariKanan3._centerPosition, jariKanan3._euler[2], -30);
                jariKanan4.rotatede(jariKanan4._centerPosition, jariKanan4._euler[2], -30);

            // kaki
            var kakiKiri = new Asset3d(new Vector3(0.8f, 0.8f, 0.8f));
            var kakiKanan = new Asset3d(new Vector3(0.8f, 0.8f, 0.8f));
            kakiKiri.createEllipsoid2(0.18f, 0.5f, 0.15f, -0.15f, -0.38f, 0, 100, 100);
            kakiKanan.createEllipsoid2(0.18f, 0.5f, 0.15f, 0.15f, -0.38f, 0f, 100, 100);

            baymax.Child.Add(mataKiri);
            baymax.Child.Add(mataKanan);
            baymax.Child.Add(garisMata);
            baymax.Child.Add(badanAtas);
            baymax.Child.Add(badanBawah);
            baymax.Child.Add(tanganKiri);
            baymax.Child.Add(tanganKanan);
            baymax.Child.Add(jariKiri1);
            baymax.Child.Add(jariKiri2);
            baymax.Child.Add(jariKiri3);
            baymax.Child.Add(jariKiri4);
            baymax.Child.Add(jariKanan1);
            baymax.Child.Add(jariKanan2);
            baymax.Child.Add(jariKanan3);
            baymax.Child.Add(jariKanan4);
            baymax.Child.Add(kakiKiri);
            baymax.Child.Add(kakiKanan);
            _objects3d.Add(baymax);
        }

        private void loadOlaf()
        {
            //kepala
            var olaf = new Asset3d(new Vector3(0.8f, 0.8f, 0.8f));
            var kepalaBawah = new Asset3d(new Vector3(0.8f, 0.8f, 0.8f));
            olaf.createEllipsoid2(0.2f, 0.35f, 0.24f, 1.9f, 0.43f, 0, 100, 100);
            kepalaBawah.createEllipsoid2(0.25f, 0.15f, 0.225f, 1.9f, 0.4f, 0, 100, 100);
            olaf.defaultPosition = new Vector3(1.9f, 0.43f, 0);

                var topimerah = new Asset3d(new Vector3(0.960f, 0.078f, 0.058f));
                var ujungtopi = new Asset3d(new Vector3(0, 0, 0));
                var topiputih = new Asset3d(new Vector3(1, 1, 1));
                topimerah.createHalfEllipsoid(0.14f, 0.28f, 0.15f, 1.9f, 0.75f, 0, 100, 100);
                ujungtopi.createEllipsoid2(0.05f, 0.05f, 0.045f, 1.97f, 1.06f, 0.01f, 100, 100);
                topiputih.createCylinder3(0.146f, 0.155f, 0.03f, 1.9f, 0.75f, 0f, 100, 100);
                topimerah.rotatede(topimerah._centerPosition, topimerah._euler[2], -10);

                //dagu
                var dagu = new Asset3d(new Vector3(0.8f, 0.8f, 0.8f));
                dagu.createHalfEllipsoid(0.25f, 0.4f, 0.18f, 1.9f, 0.4f, 0, 100, 100);
                dagu.rotatede(dagu._centerPosition, dagu._euler[0], 180);

                //mulut
                var mulut = new Asset3d(new Vector3(0.2f, 0.2f, 0.2f));
                mulut.createHalfEllipsoid(0.18f, 0.27f, 0.09f, 1.9f, 0.4f, 0.15f, 100, 100);
                mulut.rotatede(mulut._centerPosition, mulut._euler[0], 185);

                    //gigi
                    var gigi = new Asset3d(new Vector3(1, 1, 1));
                    gigi.createCuboid(0.12f, 0.07f, 0.01f, 1.9f, 0.358f, 0.235f);
                    gigi.rotatede(gigi._centerPosition, gigi._euler[0], 7);

                //hidung
                var hidung = new Asset3d(new Vector3(0.96f, 0.345f, 0.019f));
                hidung.createEllipsoid2(0.03f, 0.03f, 0.2f, 1.9f, 0.47f, 0.3f, 100, 100);

                //mata
                //dalam
                var mataDalamKiri = new Asset3d(new Vector3(0.95f, 0.95f, 0.95f));
                var mataDalamKanan = new Asset3d(new Vector3(0.95f, 0.95f, 0.95f));
                mataDalamKiri.createCylinder(0.06f, 0.01f, 1.82f, 0.55f, 0.2f);
                mataDalamKanan.createCylinder(0.06f, 0.01f, 1.98f, 0.55f, 0.2f);
                mataDalamKiri.rotatede(mataDalamKiri._centerPosition, mataDalamKiri._euler[0], -100);
                mataDalamKanan.rotatede(mataDalamKanan._centerPosition, mataDalamKanan._euler[0], -100);
                mataDalamKiri.rotatede(mataDalamKiri._centerPosition, mataDalamKiri._euler[2], 150);
                mataDalamKanan.rotatede(mataDalamKanan._centerPosition, mataDalamKanan._euler[2], -150);

                    //luar
                    var mataLuarKiri = new Asset3d(new Vector3(0, 0, 0));
                    var mataLuarKanan = new Asset3d(new Vector3(0, 0, 0));
                    mataLuarKiri.createCylinder(0.025f, 0.01f, 1.82f, 0.53f, 0.21f);
                    mataLuarKanan.createCylinder(0.025f, 0.01f, 1.98f, 0.53f, 0.21f);
                    mataLuarKiri.rotatede(mataLuarKiri._centerPosition, mataLuarKiri._euler[0], -100);
                    mataLuarKanan.rotatede(mataLuarKanan._centerPosition, mataLuarKanan._euler[0], -100);
                    mataLuarKiri.rotatede(mataLuarKiri._centerPosition, mataLuarKiri._euler[2], 150);
                    mataLuarKanan.rotatede(mataLuarKanan._centerPosition, mataLuarKanan._euler[2], -150);

                //alis
                var alisKiri = new Asset3d(new Vector3(0.1f, 0.1f, 0.1f));
                var alisKanan = new Asset3d(new Vector3(0.1f, 0.1f, 0.1f));
                alisKiri.createHalfCylinder(0.06f, 0.03f, 0.008f, 1.835f, 0.67f, 0.14f);
                alisKanan.createHalfCylinder(0.06f, 0.03f, 0.008f, 1.965f, 0.67f, 0.14f);
                alisKiri.rotatede(alisKiri._centerPosition, alisKiri._euler[0], 90);
                alisKanan.rotatede(alisKanan._centerPosition, alisKanan._euler[0], 90);
                alisKiri.rotatede(alisKiri._centerPosition, alisKiri._euler[2], -165);
                alisKanan.rotatede(alisKanan._centerPosition, alisKanan._euler[2], 165);
                alisKiri.rotatede(alisKiri._centerPosition, alisKiri._euler[1], -20);
                alisKanan.rotatede(alisKanan._centerPosition, alisKanan._euler[1], 20);

                

            //badan
            var badanAtas = new Asset3d(new Vector3(0.9f, 0.9f, 0.9f));
            var badanBawah = new Asset3d(new Vector3(0.9f, 0.9f, 0.9f));
            badanAtas.createEllipsoid2(0.23f, 0.17f, 0.18f, 1.9f, -0.03f, 0, 100, 100); 
            badanBawah.createEllipsoid2(0.37f, 0.35f, 0.27f, 1.9f, -0.37f, 0, 100, 100); 

                    //kancing
                    var kancing1 = new Asset3d(new Vector3(0.2f, 0.2f, 0.2f));
                    var kancing2 = new Asset3d(new Vector3(0.2f, 0.2f, 0.2f));
                    var kancing3 = new Asset3d(new Vector3(0.2f, 0.2f, 0.2f));
                    kancing1.createEllipsoid2(0.06f, 0.06f, 0.04f, 1.9f, 0f, 0.15f, 100, 100);
                    kancing2.createEllipsoid2(0.06f, 0.06f, 0.04f, 1.9f, -0.25f, 0.24f, 100, 100);
                    kancing3.createEllipsoid2(0.06f, 0.06f, 0.04f, 1.9f, -0.45f, 0.24f, 100, 100);

                //tangan

                //kiri
                var tanganKiri = new Asset3d(new Vector3(0.47f, 0.266f, 0.168f));
                tanganKiri.createCylinder(0.01f, 0.6f, 1.5f, 0.18f, 0);
                tanganKiri.rotatede(tanganKiri._centerPosition, tanganKiri._euler[2], 50);
                    //jari
                    var jariKiri1 = new Asset3d(new Vector3(0.47f, 0.266f, 0.168f));
                    var jariKiri2 = new Asset3d(new Vector3(0.47f, 0.266f, 0.168f));
                    var jariKiri3 = new Asset3d(new Vector3(0.47f, 0.266f, 0.168f));
                    var jariKiri4 = new Asset3d(new Vector3(0.47f, 0.266f, 0.168f));
                    jariKiri1.createCylinder(0.01f, 0.15f, 1.23f, 0.4f, 0);
                    jariKiri2.createCylinder(0.01f, 0.1f, 1.33f, 0.38f, 0);
                    jariKiri3.createCylinder(0.01f, 0.1f, 1.26f,0.34f, 0);
                    jariKiri4.createCylinder(0.01f, 0.16f, 1.29f, 0.41f, 0);
                    jariKiri1.rotatede(jariKiri1._centerPosition, jariKiri1._euler[2], 50);
                    jariKiri2.rotatede(jariKiri2._centerPosition, jariKiri2._euler[2], 145);
                    jariKiri3.rotatede(jariKiri3._centerPosition, jariKiri3._euler[2], 90);
                    jariKiri4.rotatede(jariKiri4._centerPosition, jariKiri4._euler[2], 190);

                //kanan
                var tanganKanan = new Asset3d(new Vector3(0.47f, 0.266f, 0.168f));
                tanganKanan.createCylinder(0.01f, 0.6f, 2.3f, -0.05f, 0);
                tanganKanan.rotatede(tanganKanan._centerPosition, tanganKanan._euler[2], 80);

                    //jari
                    var jariKanan1 = new Asset3d(new Vector3(0.47f, 0.266f, 0.168f));
                    var jariKanan2 = new Asset3d(new Vector3(0.47f, 0.266f, 0.168f));
                    var jariKanan3 = new Asset3d(new Vector3(0.47f, 0.266f, 0.168f));
                    var jariKanan4 = new Asset3d(new Vector3(0.47f, 0.266f, 0.168f));
                    jariKanan1.createCylinder(0.01f, 0.07f, 2.55f, -0.13f, 0);
                    jariKanan2.createCylinder(0.01f, 0.1f, 2.65f, -0.11f, 0);
                    jariKanan3.createCylinder(0.01f, 0.12f, 2.6f, -0.13f, 0);
                    jariKanan4.createCylinder(0.01f, 0.1f, 2.6f, -0.063f, 0);
                    jariKanan1.rotatede(jariKanan1._centerPosition, jariKanan1._euler[2], 30);
                    jariKanan2.rotatede(jariKanan2._centerPosition, jariKanan2._euler[2], 80);
                    jariKanan3.rotatede(jariKanan3._centerPosition, jariKanan3._euler[2], 60);
                    jariKanan4.rotatede(jariKanan4._centerPosition, jariKanan4._euler[2], 120);

            //kaki
            var kakiKiri = new Asset3d(new Vector3(0.8f, 0.8f, 0.8f));
            var kakiKanan = new Asset3d(new Vector3(0.8f, 0.8f, 0.8f));
            kakiKiri.createEllipsoid2(0.11f, 0.13f, 0.11f, 1.75f, -0.75f, 0, 100, 100); //kiri
            kakiKanan.createEllipsoid2(0.11f, 0.13f, 0.11f, 2.05f, -0.75f, 0, 100, 100); //kanan

            olaf.Child.Add(kepalaBawah);
            olaf.Child.Add(topimerah);
            olaf.Child.Add(ujungtopi);
            olaf.Child.Add(topiputih);
            olaf.Child.Add(dagu);
            olaf.Child.Add(mulut);
            olaf.Child.Add(gigi);
            olaf.Child.Add(hidung);
            olaf.Child.Add(alisKiri);
            olaf.Child.Add(alisKanan);
            olaf.Child.Add(mataDalamKiri);
            olaf.Child.Add(mataDalamKanan);
            olaf.Child.Add(mataLuarKiri);
            olaf.Child.Add(mataLuarKanan);
            olaf.Child.Add(badanAtas);
            olaf.Child.Add(badanBawah);
            olaf.Child.Add(kancing1);
            olaf.Child.Add(kancing2);
            olaf.Child.Add(kancing3);
            olaf.Child.Add(tanganKiri); 
            olaf.Child.Add(tanganKanan);
            olaf.Child.Add(jariKiri1); 
            olaf.Child.Add(jariKiri2);
            olaf.Child.Add(jariKiri3);
            olaf.Child.Add(jariKiri4);
            olaf.Child.Add(jariKanan1);
            olaf.Child.Add(jariKanan2); 
            olaf.Child.Add(jariKanan3);
            olaf.Child.Add(jariKanan4);
            olaf.Child.Add(kakiKiri);
            olaf.Child.Add(kakiKanan);

            olaf.createTranslation(new Vector3(0.8f, 0.0f, 0));
            _objects3d.Add(olaf);
        }

        private void loadEve()
        {
            //kepala
            var eve = new Asset3d();
            var kepala = new Asset3d(new Vector3(0.9f, 0.9f, 0.9f));
            var kepalaBawah = new Asset3d(new Vector3(0.9f, 0.9f, 0.9f));
            kepala.createHalfEllipsoid(0.25f, 0.28f, 0.2f, -1.9f, 0.3f, 0.0f, 100, 100);
            kepalaBawah.createEllipsoid2(0.25f, 0.13f, 0.2f, -1.9f, 0.3f, 0.0f, 100, 100);
            kepala.defaultPosition = new Vector3(-1.9f, 0.3f, 0.0f);

                //muka
                var mukaAtas = new Asset3d(new Vector3(0.05f, 0.05f, 0.05f));
                var mukaBawah = new Asset3d(new Vector3(0.05f, 0.05f, 0.05f));
                mukaAtas.createHalfEllipsoid(0.185f, 0.18f, 0.1f, -1.88f, 0.31f, 0.12f, 100, 100);
                mukaBawah.createEllipsoid2(0.185f, 0.09f, 0.1f, -1.88f, 0.31f, 0.12f, 100, 100);

                    //mata
                    var mataKiri = new Asset3d(new Vector3(0.105f, 0.45f, 0.972f));
                    var mataKanan = new Asset3d(new Vector3(0.105f, 0.45f, 0.972f));
                    mataKiri.createCylinder2(0.05f, 0.03f, 0.01f, -1.93f, 0.355f, 0.21f);
                    mataKanan.createCylinder2(0.05f, 0.03f, 0.01f, -1.79f, 0.35f, 0.2f);
                    mataKiri.rotatede(mataKiri._centerPosition, mataKiri._euler[0], 82);
                    mataKanan.rotatede(mataKanan._centerPosition, mataKanan._euler[0], 82);
                    mataKiri.rotatede(mataKiri._centerPosition, mataKiri._euler[2], 8);
                    mataKanan.rotatede(mataKanan._centerPosition, mataKanan._euler[2], -13);
                    mataKiri.rotatede(mataKiri._centerPosition, mataKiri._euler[1], -16);
                    mataKanan.rotatede(mataKanan._centerPosition, mataKanan._euler[1], 14);
                    mataKiri.defaultPosition = mataKiri._centerPosition;

            //badan
            var badan = new Asset3d(new Vector3(0.9f, 0.9f, 0.9f));
            badan.createHalfEllipsoid(0.25f, 0.75f, 0.2f, -1.9f, 0.05f, 0, 100, 100);
            badan.rotatede(badan._centerPosition, badan._euler[0], 180);

            //tangan
            var tanganKiri = new Asset3d(new Vector3(0.8f, 0.8f, 0.8f));
            var tanganKanan = new Asset3d(new Vector3(0.8f, 0.8f, 0.8f));
            tanganKiri.createEllipsoid2(0.08f, 0.375f, 0.11f, -2.6f, -0.25f, 0, 100, 100);
            tanganKanan.createEllipsoid2(0.08f, 0.375f, 0.11f, -1.3f, -0.3f, 0, 100, 100);
            tanganKiri.rotatede(tanganKiri._centerPosition, tanganKiri._euler[2], -50);
            tanganKanan.rotatede(tanganKanan._centerPosition, tanganKanan._euler[2], 20);

            kepala.Child.Add(kepalaBawah);
            kepala.Child.Add(mukaAtas);
            kepala.Child.Add(mukaBawah);
            kepala.Child.Add(mataKiri);
            kepala.Child.Add(mataKanan);
            eve.Child.Add(kepala);
            eve.Child.Add(badan);
            eve.Child.Add(tanganKiri);
            eve.Child.Add(tanganKanan);
            _objects3d.Add(eve);
        }

        private void loadAlas()
        {
            //atas
            var alas = new Asset3d(new Vector3(1, 1, 1));
            var alasAtas2 = new Asset3d(new Vector3(1, 1, 1));
            alas.createCylinder3(3.3f, 2.9f, 0.03f, 0, -0.85f, 0.0f, 100, 100);
            alasAtas2.createEllipsoid2(3.3f, 0, 2.9f, 0, -0.85f, 0.0f, 100, 100);

            //tengah
            var alasTengah = new Asset3d(new Vector3(0.47f, 0.266f, 0.168f));
            alasTengah.createHalfEllipsoid(3.5f, 0.5f, 3.1f, 0, -0.8f, 0.0f, 100, 100);
            alasTengah.rotatede(alasTengah._centerPosition, alasTengah._euler[0], 180);

            var alasBawah = new Asset3d(new Vector3(1, 1, 1));
            alasBawah.createEllipsoid2(12.9f, 0.5f, 11.9f, 0, -1.25f, -8.5f, 100, 100);

            alas.Child.Add(alasAtas2);
            alas.Child.Add(alasTengah);
            alas.Child.Add (alasBawah);
            _objects3d.Add(alas);
        }

        private void loadSnow(int snowCount)
        {
            var snow = new Asset3d(new Vector3(1, 1, 1));

            for (int i = 0; i < snowCount; i++)
            {
                float x = NextFloat(-2.5f, 2.5f);
                float y = NextFloat(-0.5f, 2);
                float z = NextFloat(-1.8f, 1.8f);
                float sizeX = NextFloat(0, 0.03f);
                float sizeY = NextFloat(0, 0.03f);
                float sizeZ = NextFloat(0, 0.03f);

                snow.Child.Add(new Asset3d(new Vector3(1, 1, 1)));
                snow.Child[i].createEllipsoid2(sizeX, sizeY, sizeZ, x, y, z, 100, 100);
            }

            _objects3d.Add(snow);
        }

        private float NextFloat(float min, float max)
        {
            System.Random random = new System.Random();
            double val = (random.NextDouble() * (max - min) + min);
            return (float)val;
        }

        private void loadCloud()
        {
            var cloudkiri = new Asset3d();
            var cloudkanan = new Asset3d();
            var cloud = new Asset3d();
            var cloud1 = new Asset3d(new Vector3(1, 1, 1));
            var cloud2 = new Asset3d(new Vector3(1, 1, 1));
            var cloud3 = new Asset3d(new Vector3(1, 1, 1));
            var cloud4 = new Asset3d(new Vector3(1, 1, 1));
            var cloud5 = new Asset3d(new Vector3(1, 1, 1));
            var cloud6 = new Asset3d(new Vector3(1, 1, 1));
            cloud1.createEllipsoid2(1.2f, 0.8f, 1, -1, 3f, -5.5f, 100, 100);
            cloud2.createEllipsoid2(1.5f, 0.5f, 0.8f, -2.5f, 3, -5.5f, 100, 100);
            cloud3.createEllipsoid2(1.3f, 0.6f, 0.7f, 0.5f, 3, -5.5f, 100, 100);
            cloud4.createEllipsoid2(1.2f, 0.8f, 1, 1, 7f, -5.5f, 100, 100);
            cloud5.createEllipsoid2(1.5f, 0.5f, 0.8f, -0.5f, 7, -5.5f, 100, 100);
            cloud6.createEllipsoid2(1.3f, 0.6f, 0.7f, 2.5f, 7, -5.5f, 100, 100);

            cloudkiri.Child.Add(cloud1);
            cloudkiri.Child.Add(cloud2);
            cloudkiri.Child.Add(cloud3);
            cloudkanan.Child.Add(cloud4);
            cloudkanan.Child.Add(cloud5);
            cloudkanan.Child.Add(cloud6);

            cloud.Child.Add(cloudkiri);
            cloud.Child.Add(cloudkanan);

            _objects3d.Add(cloud);
        }

        private void loadTrees()
        {
            var trees = new Asset3d();

            var tree1 = new Asset3d(new Vector3(0.701f, 0.349f, 0.258f));
            var daun1Tree1 = new Asset3d(new Vector3(0.803f, 0.933f, 0.749f));
            var daun2Tree1 = new Asset3d(new Vector3(0.501f, 0.862f, 0.337f));
            var daun3Tree1 = new Asset3d(new Vector3(0.341f, 0.556f, 0.243f));

            tree1.createCylinder3(0.1f, 0.1f, 1, 0, 0.5f, -1.6f, 100, 100);
            daun1Tree1.createHalfEllipsoid(0.4f, 3.5f, 0.43f, 0, 0.3f, -1.6f, 100, 100);
            daun2Tree1.createHalfEllipsoid(0.7f, 2f, 0.73f, 0, 0.5f, -1.6f, 100, 100);
            daun3Tree1.createHalfEllipsoid(1, 1.5f, 1, 0, -0.2f, -1.6f, 100, 100);

            var tree2 = new Asset3d(new Vector3(0.701f, 0.349f, 0.258f));
            var daun1Tree2 = new Asset3d(new Vector3(0.803f, 0.933f, 0.749f));
            var daun2Tree2 = new Asset3d(new Vector3(0.501f, 0.862f, 0.337f));
            var daun3Tree2 = new Asset3d(new Vector3(0.341f, 0.556f, 0.243f));

            tree2.createCylinder3(0.1f, 0.1f, 1, 0, 0.5f, -1.6f, 100, 100);
            daun1Tree2.createHalfEllipsoid(0.4f, 3.5f, 0.43f, 0, 0.3f, -1.6f, 100, 100);
            daun2Tree2.createHalfEllipsoid(0.7f, 2f, 0.73f, 0, 0.5f, -1.6f, 100, 100);
            daun3Tree2.createHalfEllipsoid(1, 1.5f, 1, 0, -0.2f, -1.6f, 100, 100);

            var tree3 = new Asset3d(new Vector3(0.701f, 0.349f, 0.258f));
            var daun1Tree3 = new Asset3d(new Vector3(0.803f, 0.933f, 0.749f));
            var daun2Tree3 = new Asset3d(new Vector3(0.501f, 0.862f, 0.337f));
            var daun3Tree3 = new Asset3d(new Vector3(0.341f, 0.556f, 0.243f));

            tree3.createCylinder3(0.1f, 0.1f, 1, 0, 0.5f, -1.6f, 100, 100);
            daun1Tree3.createHalfEllipsoid(0.4f, 3.5f, 0.43f, 0, 0.3f, -1.6f, 100, 100);
            daun2Tree3.createHalfEllipsoid(0.7f, 2f, 0.73f, 0, 0.5f, -1.6f, 100, 100);
            daun3Tree3.createHalfEllipsoid(1, 1.5f, 1, 0, -0.2f, -1.6f, 100, 100);

            tree1.Child.Add(daun1Tree1);
            tree1.Child.Add(daun2Tree1);
            tree1.Child.Add(daun3Tree1);

            tree2.Child.Add(daun1Tree2);
            tree2.Child.Add(daun2Tree2);
            tree2.Child.Add(daun3Tree2);

            tree3.Child.Add(daun1Tree3);
            tree3.Child.Add(daun2Tree3);
            tree3.Child.Add(daun3Tree3);

            tree1.createTranslation(new Vector3(-1.2f,0.1f,0));
            tree2.createTranslation(new Vector3(0, 0.2f, -0.4f));
            tree3.createTranslation(new Vector3(1.2f, 0.1f, 0));

            trees.Child.Add(tree1);
            trees.Child.Add(tree2);
            trees.Child.Add(tree3);

            _objects3d.Add(trees);
        }

        private void loadRocks()
        {
            var rocks = new Asset3d(new Vector3(0.6f, 0.6f, 0.6f));
            var rock1 = new Asset3d(new Vector3(0.6f, 0.6f, 0.6f));
            var rock2 = new Asset3d(new Vector3(0.6f, 0.6f, 0.6f));
            var rock3 = new Asset3d(new Vector3(0.6f, 0.6f, 0.6f));
            var rock4 = new Asset3d(new Vector3(0.6f, 0.6f, 0.6f));
            var rock5 = new Asset3d(new Vector3(0.6f, 0.6f, 0.6f));
            var rock6 = new Asset3d(new Vector3(0.6f, 0.6f, 0.6f));
            var rock7 = new Asset3d(new Vector3(0.6f, 0.6f, 0.6f));
            var rock8 = new Asset3d(new Vector3(0.6f, 0.6f, 0.6f));
            var rock9 = new Asset3d(new Vector3(0.6f, 0.6f, 0.6f));
            var rock10 = new Asset3d(new Vector3(0.6f, 0.6f, 0.6f));
            var rock11 = new Asset3d(new Vector3(0.6f, 0.6f, 0.6f));
            var rock12 = new Asset3d(new Vector3(0.6f, 0.6f, 0.6f));
            var rock13 = new Asset3d(new Vector3(0.6f, 0.6f, 0.6f));
            var rock14 = new Asset3d(new Vector3(0.6f, 0.6f, 0.6f));
            var rock15 = new Asset3d(new Vector3(0.6f, 0.6f, 0.6f));
            var rock16 = new Asset3d(new Vector3(0.6f, 0.6f, 0.6f));
            var rock17 = new Asset3d(new Vector3(0.6f, 0.6f, 0.6f));
            var rock18 = new Asset3d(new Vector3(0.6f, 0.6f, 0.6f));
            var rock19 = new Asset3d(new Vector3(0.6f, 0.6f, 0.6f));
            var rock20 = new Asset3d(new Vector3(0.6f, 0.6f, 0.6f));
            var rock21 = new Asset3d(new Vector3(0.6f, 0.6f, 0.6f));
            var rock22 = new Asset3d(new Vector3(0.6f, 0.6f, 0.6f));

            rocks.createEllipsoid2(0.2f, 0.15f, 0.18f, 0.4f, -0.7f, 1.78f, 100, 100);
            rock1.createEllipsoid2(0.2f, 0.15f, 0.18f, 0, -0.7f, 1.8f, 100, 100);
            rock2.createEllipsoid2(0.2f, 0.15f, 0.18f, -0.4f, -0.7f, 1.8f, 100, 100);
            rock3.createEllipsoid2(0.2f, 0.15f, 0.18f, -0.77f, -0.7f, 1.7f, 100, 100);
            rock4.createEllipsoid2(0.2f, 0.15f, 0.18f, -1.12f, -0.7f, 1.55f, 100, 100);
            rock5.createEllipsoid2(0.2f, 0.15f, 0.18f, -1.44f, -0.7f, 1.35f, 100, 100);
            rock6.createEllipsoid2(0.2f, 0.15f, 0.18f, -1.67f, -0.7f, 1.1f, 100, 100);
            rock7.createEllipsoid2(0.2f, 0.15f, 0.18f, 0.8f, -0.7f, 1.7f, 100, 100);
            rock8.createEllipsoid2(0.2f, 0.15f, 0.18f, 1.4f, -0.7f, 1.33f, 100, 100);
            rock9.createEllipsoid2(0.2f, 0.15f, 0.18f, 1.1f, -0.7f, 1.55f, 100, 100);
            rock10.createEllipsoid2(0.2f, 0.15f, 0.18f, -1.87f, -0.7f, 0.8f, 100, 100);
            rock11.createEllipsoid2(0.2f, 0.15f, 0.18f, 1.65f, -0.7f, 1.1f, 100, 100);
            rock12.createEllipsoid2(0.2f, 0.15f, 0.18f, -0.03f, -0.7f, 1.3f, 100, 100);
            rock13.createEllipsoid2(0.2f, 0.15f, 0.18f, -0.03f, -0.7f, 0.9f, 100, 100);
            rock14.createEllipsoid2(0.2f, 0.15f, 0.18f, -0.03f, -0.7f, 0.4f, 100, 100);
            rock15.createEllipsoid2(0.2f, 0.15f, 0.18f, 0.35f, -0.7f, 0.45f, 100, 100);
            rock16.createEllipsoid2(0.2f, 0.15f, 0.18f, 0.7f, -0.75f, 0.48f, 100, 100);
            rock17.createEllipsoid2(0.2f, 0.15f, 0.18f, 1.07f, -0.75f, 0.65f, 100, 100);
            rock18.createEllipsoid2(0.2f, 0.15f, 0.18f, 1.45f, -0.75f, 0.82f, 100, 100);
            rock19.createEllipsoid2(0.2f, 0.15f, 0.18f, -0.39f, -0.7f, 0.47f, 100, 100);
            rock20.createEllipsoid2(0.2f, 0.15f, 0.18f, -0.75f, -0.7f, 0.54f, 100, 100);
            rock21.createEllipsoid2(0.2f, 0.15f, 0.18f, -1.1f, -0.7f, 0.58f, 100, 100);
            rock22.createEllipsoid2(0.2f, 0.15f, 0.18f, -1.5f, -0.7f, 0.62f, 100, 100);

            rock2.rotatede(rock2._centerPosition, rock2._euler[0], 180);
            rock2.rotatede(rock2._centerPosition, rock2._euler[0], 180);
            rock2.rotatede(rock2._centerPosition, rock2._euler[0], 180);
            rock2.rotatede(rock2._centerPosition, rock2._euler[0], 180);
            rock3.rotatede(rock3._centerPosition, rock3._euler[0], 170);
            rock4.rotatede(rock4._centerPosition, rock4._euler[0], 175);
            rock5.rotatede(rock5._centerPosition, rock5._euler[0], 175);
            rock6.rotatede(rock6._centerPosition, rock6._euler[0], 175);

            rocks.Child.Add(rock1);
            rocks.Child.Add(rock2);
            rocks.Child.Add(rock3);
            rocks.Child.Add(rock4);
            rocks.Child.Add(rock5);
            rocks.Child.Add(rock6);
            rocks.Child.Add(rock7);
            rocks.Child.Add(rock8);
            rocks.Child.Add(rock9);
            rocks.Child.Add(rock10);
            rocks.Child.Add(rock11);
            rocks.Child.Add(rock12);
            rocks.Child.Add(rock13);
            rocks.Child.Add(rock14);
            rocks.Child.Add(rock15);
            rocks.Child.Add(rock16);
            rocks.Child.Add(rock17);
            rocks.Child.Add(rock18);
            rocks.Child.Add(rock19);
            rocks.Child.Add(rock20);
            rocks.Child.Add(rock21);
            rocks.Child.Add(rock22);

            _objects3d.Add(rocks);
        }

        private void loadMountains()
        {
            var mountains = new Asset3d();

            var mount1 = new Asset3d(new Vector3(1, 1, 1));
            mount1.createHalfEllipsoid(4.4f, 9.5f, 3.43f, 0, -0.9f, -6.6f, 100, 100);

            var mount2 = new Asset3d(new Vector3(0.96f, 0.96f, 0.96f));
            mount2.createHalfEllipsoid(2.4f, 5.5f, 3.43f, 5f, -0.9f, -4.6f, 100, 100);

            var mount3 = new Asset3d(new Vector3(0.96f, 0.96f, 0.96f));
            mount3.createHalfEllipsoid(2.4f, 5.5f, 3.43f, -5f, -0.9f, -4.6f, 100, 100);


            mountains.Child.Add(mount1);
            mountains.Child.Add(mount2);
            mountains.Child.Add(mount3);

            _objects3d.Add(mountains);

        }

        private void loadFlowers()
        {
            var flowers = new Asset3d();

            //Bunga1
            //Bulatan tengah
            var bunga1 = new Asset3d(new Vector3(1, 1, 1));
            bunga1.createEllipsoid2(0.15f, 0.15f, 0.1f, -2.3f, -0.3f, 1, 100, 100);

            //Kelopak1
            var kelopak1 = new Asset3d(new Vector3(1, 0, 1));
            kelopak1.createHalfEllipsoid(0.1f, 0.2f, 0.09f, -2.3f, -0.21f, 1f, 100, 100);

            //Kelopak2
            var kelopak2 = new Asset3d(new Vector3(1, 0, 1));
            kelopak2.createHalfEllipsoid(0.1f, 0.2f, 0.09f, -2.4f, -0.28f, 1f, 100, 100);
            kelopak2.rotatede(kelopak2._centerPosition, kelopak2._euler[2], 75);

            //Kelopak3
            var kelopak3 = new Asset3d(new Vector3(1, 0, 1));
            kelopak3.createHalfEllipsoid(0.1f, 0.2f, 0.09f, -2.35f, -0.4f, 1f, 100, 100);
            kelopak3.rotatede(kelopak3._centerPosition, kelopak3._euler[2], 145);

            //Kelopak4
            var kelopak4 = new Asset3d(new Vector3(1, 0, 1));
            kelopak4.createHalfEllipsoid(0.1f, 0.2f, 0.09f, -2.23f, -0.4f, 1f, 100, 100);
            kelopak4.rotatede(kelopak4._centerPosition, kelopak4._euler[2], 230);

            //Kelopak4
            var kelopak5 = new Asset3d(new Vector3(1, 0, 1));
            kelopak5.createHalfEllipsoid(0.1f, 0.2f, 0.09f, -2.19f, -0.27f, 1f, 100, 100);
            kelopak5.rotatede(kelopak5._centerPosition, kelopak5._euler[2], 295);

            //Tangkai
            var tangkai = new Asset3d(new Vector3(0.47f, 0.266f, 0.168f));
            tangkai.createCylinder(0.02f, 0.6f, -2.3f, -0.62f, 1f);
            tangkai.rotatede(tangkai._centerPosition, tangkai._euler[2], 0);

            //Bunga2
            //Bulatan Tengah
            var bunga2 = new Asset3d(new Vector3(1, 1, 1));
            bunga2.createEllipsoid2(0.15f, 0.15f, 0.1f, 2.2f, -0.3f, 1, 100, 100);

            //Kelopak6
            var kelopak6 = new Asset3d(new Vector3(1, 0, 0));
            kelopak6.createHalfEllipsoid(0.1f, 0.2f, 0.09f, 2.2f, -0.21f, 1f, 100, 100);

            //Kelopak7
            var kelopak7 = new Asset3d(new Vector3(1, 0, 0));
            kelopak7.createHalfEllipsoid(0.1f, 0.2f, 0.09f, 2.1f, -0.28f, 1f, 100, 100);
            kelopak7.rotatede(kelopak7._centerPosition, kelopak7._euler[2], 75);

            //Kelopak8
            var kelopak8 = new Asset3d(new Vector3(1, 0, 0));
            kelopak8.createHalfEllipsoid(0.1f, 0.2f, 0.09f, 2.15f, -0.38f, 1f, 100, 100);
            kelopak8.rotatede(kelopak8._centerPosition, kelopak8._euler[2], 145);

            //Kelopak9
            var kelopak9 = new Asset3d(new Vector3(1, 0, 0));
            kelopak9.createHalfEllipsoid(0.1f, 0.2f, 0.09f, 2.27f, -0.39f, 1f, 100, 100);
            kelopak9.rotatede(kelopak9._centerPosition, kelopak9._euler[2], 230);

            //Kelopak10
            var kelopak10 = new Asset3d(new Vector3(1, 0, 0));
            kelopak10.createHalfEllipsoid(0.1f, 0.2f, 0.09f, 2.31f, -0.27f, 1f, 100, 100);
            kelopak10.rotatede(kelopak10._centerPosition, kelopak10._euler[2], 293);

            //Tangkai2
            var tangkai2 = new Asset3d(new Vector3(0.47f, 0.266f, 0.168f));
            tangkai2.createCylinder(0.02f, 0.6f, 2.21f, -0.62f, 1f);
            tangkai2.rotatede(tangkai2._centerPosition, tangkai2._euler[2], 0);



            bunga1.Child.Add(kelopak1);
            bunga1.Child.Add(kelopak2);
            bunga1.Child.Add(kelopak3);
            bunga1.Child.Add(kelopak4);
            bunga1.Child.Add(kelopak5);
            bunga1.Child.Add(tangkai);
            bunga2.Child.Add(kelopak6);
            bunga2.Child.Add(kelopak7);
            bunga2.Child.Add(kelopak8);
            bunga2.Child.Add(kelopak9);
            bunga2.Child.Add(kelopak10);
            bunga2.Child.Add(tangkai2);

            flowers.Child.Add(bunga1);
            flowers.Child.Add(bunga2);
            _objects3d.Add(flowers);
        }
    }
}
