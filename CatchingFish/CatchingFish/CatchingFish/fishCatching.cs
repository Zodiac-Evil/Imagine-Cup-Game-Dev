using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;
using Microsoft.Devices.Sensors;

namespace CatchingFish
{
    /// <summary>
    /// 这是游戏的主类型
    /// </summary>
    public class fishCatching : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Accelerometer accelerometer;
        public static Random rand = new Random();

        class Fish
        {
            public Texture2D fish_pic;

            public Vector2 position;

            public Vector2 speed;

            public Color color;

            public Boolean isCaught;

            public Fish(float x, float y, Boolean caught)
            {
                position = new Vector2(x, y);

                speed = new Vector2(rand.Next(-480, 480), rand.Next(-800, 800));

                color = Color.White;

                isCaught = caught;
            }
        }


        private int count = 0;
        private int lastCount = 0;
        private int fishCount = 0;//捕到的鱼总数

        SpriteFont scoreFont;

        List<Fish> fishes;

        Texture2D logo;//渔网
        Texture2D pause;//暂停按钮
        Texture2D play;//继续按钮
        Texture2D backgroundTexture;//背景图片

        Vector2 logoPosition;//渔网的坐标
        Vector2 logoVelocity;//渔网的速度

        //Vector2 endpoint = new Vector2(200, 300);
        //Vector2 endpoint01 = new Vector2(200, 300);

        SoundEffect soundEffect;

        public fishCatching()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.PreferredBackBufferWidth = 480;
            graphics.PreferredBackBufferHeight = 800;

            fishes = new List<Fish>();

            // Windows Phone 的默认帧速率为 30 fps。
            TargetElapsedTime = TimeSpan.FromTicks(33333);

            // 延长锁定时的电池寿命。
            InactiveSleepTime = TimeSpan.FromSeconds(1);
        }

        /// <summary>
        /// 允许游戏在开始运行之前执行其所需的任何初始化。
        /// 游戏能够在此时查询任何所需服务并加载任何非图形
        /// 相关的内容。调用 base.Initialize 将枚举所有组件
        /// 并对其进行初始化。 
        /// </summary>
        protected override void Initialize()
        {
            // TODO: 在此处添加初始化逻辑
            if (!Accelerometer.IsSupported)
            {//检测设备是否支持重力感应，不支持则抛出异常
                throw new Exception("Device does not support accelerometer!");
            }

            if (accelerometer == null)
            {//初始化accelerometer
                accelerometer = new Accelerometer();

                //设置感应器检测间隔 单位是毫秒 （The default value is 2 milliseconds.[MSDN]）
                accelerometer.TimeBetweenUpdates = TimeSpan.FromMilliseconds(20);

                //[个人理解] 添加监听事件的回调函数 （accelerometer_CurrentValueChanged),
                //          即每次检测的到的结果交给此方法进行处理（本例中用来更新四个方向指示属性）
                accelerometer.ReadingChanged += new EventHandler<AccelerometerReadingEventArgs>(AccelerometerReadingChanged);
            }

            try
            {
                //开始感应器检测
                accelerometer.Start();
            }
            catch (InvalidOperationException ex)
            {
                throw new Exception("Unable to start accelerometer", ex);
            }

            base.Initialize();
        }

        void AccelerometerReadingChanged(object sender, AccelerometerReadingEventArgs e)
        {//改变捕鱼网的位置
            logoVelocity.X += (float)e.X;
            logoVelocity.Y += -(float)e.Y;
            logoPosition += logoVelocity;
        }

        /// <summary>
        /// 对于每个游戏会调用一次 LoadContent，
        /// 用于加载所有内容。
        /// </summary>
        protected override void LoadContent()
        {
            // 创建新的 SpriteBatch，可将其用于绘制纹理。
            spriteBatch = new SpriteBatch(GraphicsDevice);
            ContentManager cm = this.Content;
            scoreFont = cm.Load<SpriteFont>("scoreFont");
            logo = Content.Load<Texture2D>("cat_net");
            Viewport viewport = graphics.GraphicsDevice.Viewport;
            logoPosition = new Vector2((viewport.Width - logo.Width)/2, (viewport.Height - logo.Height)/2);

            fishes.Add(new Fish(20, 50, false));
            fishes.Add(new Fish(30, 50, false));
            fishes.Add(new Fish(75, 100, false));
            fishes.Add(new Fish(200, 500, false));
            fishes.Add(new Fish(92, 70, false));
            fishes.Add(new Fish(30, 80, false));
            fishes.Add(new Fish(25, 85, false));
            fishes.Add(new Fish(200, 100, false));

            foreach (Fish each in fishes)
            {
                each.fish_pic = Content.Load<Texture2D>("fish");
            }

            soundEffect = Content.Load<SoundEffect>("Windows Ding");

            backgroundTexture = Content.Load<Texture2D>("Background");

            pause = Content.Load<Texture2D>("pause");
            play = Content.Load<Texture2D>("play");
            
            // TODO: 在此处使用 this.Content 加载游戏内容
            
        }

        /// <summary>
        /// 对于每个游戏会调用一次 UnloadContent，
        /// 用于取消加载所有内容。
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: 在此处取消加载任何非 ContentManager 内容
            accelerometer.Stop();
        }


        /// <summary>
        /// 允许游戏运行逻辑，例如更新全部内容、
        /// 检查冲突、收集输入信息以及播放音频。
        /// </summary>
        /// <param name="gameTime">提供计时值的快照。</param>
        protected override void Update(GameTime gameTime)
        {
            // 允许游戏退出
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            // TODO: 在此处添加更新逻辑
            
            float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            float totalTime = (float)gameTime.TotalGameTime.TotalSeconds;

            Viewport viewport = graphics.GraphicsDevice.Viewport;

            
            

            //鱼不能超过边界
            foreach(Fish fish_each in fishes){
                if (fish_each.position.X < 0)
            {
                fish_each.speed.X *= -1;
                fish_each.position += fish_each.speed * elapsedTime;
            }
            else
                    if (fish_each.position.X > viewport.Width - fish_each.fish_pic.Width)
                {
                    fish_each.speed.X *= -1;
                    fish_each.position += fish_each.speed * elapsedTime;
                }

                if (fish_each.position.Y < 0)
                {
                    fish_each.speed.Y *= -1;
                    fish_each.position += fish_each.speed * elapsedTime;
                }
                else
                {
                    if (fish_each.position.Y >= viewport.Height - fish_each.fish_pic.Height)
                    {
                        fish_each.speed.Y *= -1;
                        fish_each.position += fish_each.speed * elapsedTime;
                    }
                }

                //捕到鱼了
                if ((logoPosition.X - fish_each.position.X) >= -logo.Width && (logoPosition.X - fish_each.position.X) <= fish_each.fish_pic.Width && (logoPosition.Y - fish_each.position.Y) >= -logo.Height && (logoPosition.Y - fish_each.position.Y) <= fish_each.fish_pic.Height)
                {//捕到鱼了
                    fish_each.position.X = logoPosition.X + logo.Width / 2;
                    fish_each.position.Y = logoPosition.Y + logo.Height / 2;
                    SoundEffectInstance re = soundEffect.CreateInstance();
                    fish_each.isCaught = true;//将捕到的鱼标记为true
                    fishes.Remove(fish_each);//移除捕到的鱼
                    fishCount++;
                    re.Play();
                    re.Pause();

                }
                else
                {//没捕到鱼
                    this.count++;

                    if (this.count > 8)
                    {
                        this.lastCount++;
                        if (this.lastCount > 10)
                        {//15帧之后换一次方向
                            fish_each.speed = new Vector2(rand.Next(-480, 480), rand.Next(-800, 800));
                            this.lastCount = 0;
                        }

                        fish_each.position += fish_each.speed * elapsedTime;
                        this.count = 0;
                    }
                }

                if (fishes.Count() < 10)
                {
                    fishes.Add(new Fish(rand.Next(0, 480), rand.Next(0, 800), false));
                }

            }
            

            


            //捕鱼网不能超过边界
            if (logoPosition.X < 0)
            {
                logoPosition.X = 0;
                logoVelocity.X = 0;
            }
            else
                if(logoPosition.X > viewport.Width - logo.Width)
                {
                    logoPosition.X = viewport.Width - logo.Width;
                    logoVelocity.X = 0;
                }

            if (logoPosition.Y < 0)
            {
                logoPosition.Y = 0;
                logoVelocity.Y = 0;
            }
            else
                if (logoPosition.Y > viewport.Height - logo.Height)
                {
                    logoPosition.Y = viewport.Height - logo.Height;
                    logoVelocity.Y = 0;
                }
        }

        /// <summary>
        /// 当游戏该进行自我绘制时调用此项。
        /// </summary>
        /// <param name="gameTime">提供计时值的快照。</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.White);

            // TODO: 在此处添加绘图代码
            spriteBatch.Draw(backgroundTexture, new Rectangle(0, 0, Window.ClientBounds.Width, Window.ClientBounds.Height), null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0);

            base.Draw(gameTime);
            spriteBatch.Begin();
            spriteBatch.Draw(logo, logoPosition, Color.White);
            foreach(Fish each in fishes)
            {
                if (!each.isCaught)
                {
                    spriteBatch.Draw(each.fish_pic, each.position, each.color);
                }
                
            }

            spriteBatch.DrawString(scoreFont, "捕到的鱼:" + fishCount, new Vector2(10, 10), Color.DarkBlue, 0, Vector2.Zero, 1, SpriteEffects.None, 1);
            
            spriteBatch.End();
        }
    }
}
