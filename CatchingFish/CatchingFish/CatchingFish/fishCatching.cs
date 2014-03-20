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
using System.IO.IsolatedStorage;

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

            public int f_count;//过一段时间生成新鱼

            public Fish(float x, float y, Boolean caught)
            {
                position = new Vector2(x, y);

                speed = new Vector2(rand.Next(-480, 480), rand.Next(-800, 800));

                color = Color.White;

                isCaught = caught;
                f_count = 0;
            }
        }


        private int count = 0;
        private int lastCount = 0;
        private int fishCount = 0;//捕到的鱼总数

        

        SpriteFont scoreFont;

        Fish[] fishes = new Fish[8];//一定不要改

        Texture2D logo;//渔网
        Texture2D pause;//暂停按钮
        Texture2D play;//继续按钮
        Texture2D backgroundTexture;//背景图片

        Vector2 logoPosition;//渔网的坐标
        Vector2 logoVelocity;//渔网的速度

        //Vector2 endpoint = new Vector2(200, 300);
        //Vector2 endpoint01 = new Vector2(200, 300);

        //SoundEffect soundEffect;

        public fishCatching()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.PreferredBackBufferWidth = 480;
            graphics.PreferredBackBufferHeight = 800;

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
            fishes[0] = new Fish(20, 20, false);
            fishes[1] = new Fish(200, 700, false);
            fishes[2] = new Fish(100, 480, false);
            fishes[3] = new Fish(50, 400, false);
            fishes[4] = new Fish(200, 40, false);
            fishes[5] = new Fish(200, 400, false);
            fishes[6] = new Fish(287, 180, false);
            fishes[7] = new Fish(160, 280, false);

            //open isolated storage, and load data from the savefile if it exists.
#if WINDOWS_PHONE
            using (IsolatedStorageFile savegameStorage = IsolatedStorageFile.GetUserStoreForApplication())
#else
            using (IsolatedStorageFile savegameStorage = IsolatedStorageFile.GetUserStoreForDomain())
#endif
            {
                if (savegameStorage.FileExists("fish_amount"))
                {
                    using (IsolatedStorageFileStream fs = savegameStorage.OpenFile("fish_amount", System.IO.FileMode.Open))
                    {
                        if (fs != null)
                        {
                            //Reload the saved high-score data.
                            byte[] saveBytes = new byte[4];
                            int count = fs.Read(saveBytes, 0, 4);
                            if (count > 0)
                            {
                                fishCount = System.BitConverter.ToInt32(saveBytes, 0);
                            }
                        }
                    }
                }
            }

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
            scoreFont = Content.Load<SpriteFont>("Times New Roman");
            logo = Content.Load<Texture2D>("cat_net");
            Viewport viewport = graphics.GraphicsDevice.Viewport;
            logoPosition = new Vector2((viewport.Width - logo.Width)/2, (viewport.Height - logo.Height)/2);

            for (int i = 0; i < 8; i++)
            {
                fishes[i].fish_pic = Content.Load<Texture2D>("fish");
            }

            //soundEffect = Content.Load<SoundEffect>("Windows Ding");

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

            if (totalTime == 60)
            {
                Exit();
            }

            //鱼不能超过边界
            for (int i = 0; i < 8; i++)
            {
                if (fishes[i].f_count >= 1 && fishes[i].f_count == 600)
                {//累积到600就可以重新画出这条鱼
                    fishes[i].f_count = 0;
                    fishes[i].isCaught = false;
                }
                else
                {
                    if (fishes[i].f_count == 0)
                    {
                        fishes[i].f_count = 0;//do nothing
                    }
                    else
                    {
                        fishes[i].f_count++;
                    }
                }

                if (fishes[i].position.X < 0)
                {
                    fishes[i].speed.X *= -1;
                    fishes[i].position += fishes[i].speed * elapsedTime;
                }
                else
                    if (fishes[i].position.X > viewport.Width - fishes[i].fish_pic.Width)
                    {
                        fishes[i].speed.X *= -1;
                        fishes[i].position += fishes[i].speed * elapsedTime;
                    }

                if (fishes[i].position.Y < 0)
                {
                    fishes[i].speed.Y *= -1;
                    fishes[i].position += fishes[i].speed * elapsedTime;
                }
                else
                {
                    if (fishes[i].position.Y >= viewport.Height - fishes[i].fish_pic.Height)
                    {
                        fishes[i].speed.Y *= -1;
                        fishes[i].position += fishes[i].speed * elapsedTime;
                    }
                }

                //捕到鱼了
                if ((logoPosition.X - fishes[i].position.X) >= -logo.Width && (logoPosition.X - fishes[i].position.X) <= fishes[i].fish_pic.Width && (logoPosition.Y - fishes[i].position.Y) >= -logo.Height && (logoPosition.Y - fishes[i].position.Y) <= fishes[i].fish_pic.Height)
                {//捕到鱼了
                    fishes[i].position.X = logoPosition.X + logo.Width / 2;
                    fishes[i].position.Y = logoPosition.Y + logo.Height / 2;
                    //SoundEffectInstance re = soundEffect.CreateInstance();
                    fishes[i].isCaught = true;//将捕到的鱼标记为true
                    fishCount++;
                    fishes[i].position = new Vector2(rand.Next(0, 480), rand.Next(0, 800));
                    fishes[i].f_count++;
                }
                else
                {//没捕到鱼
                    this.count++;

                    if (this.count > 8)
                    {
                        this.lastCount++;
                        if (this.lastCount > 10)
                        {//15帧之后换一次方向
                            fishes[i].speed = new Vector2(rand.Next(-480, 480), rand.Next(-800, 800));
                            this.lastCount = 0;
                        }

                        fishes[i].position += fishes[i].speed * elapsedTime;
                        this.count = 0;
                    }
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
			base.Draw(gameTime);
            spriteBatch.Begin();
			
            spriteBatch.Draw(backgroundTexture, new Rectangle(0, 0, Window.ClientBounds.Width, Window.ClientBounds.Height), null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0);

            
            spriteBatch.Draw(logo, logoPosition, Color.White);
            for (int i = 0; i < 8; i++)
            {
                if (!fishes[i].isCaught)
                {
                    spriteBatch.Draw(fishes[i].fish_pic, fishes[i].position, fishes[i].color);
                }
                /*else
                {
                    spriteBatch.DrawString(scoreFont, "+1", new Vector2(logoPosition.X + 10, logoPosition.Y - 10), Color.Yellow, 0, Vector2.Zero, 1, SpriteEffects.None, 1);
                }*/

            }
            spriteBatch.DrawString(scoreFont, "Fishes caught: " + fishCount, new Vector2(10, 10), Color.Yellow, 0, Vector2.Zero, 1, SpriteEffects.None, 1);

            spriteBatch.End();
        }

        protected override void OnExiting(object sender, EventArgs args)
        {
            //save the game state
#if WINDOWS_PHONE
            IsolatedStorageFile savegameStorage = IsolatedStorageFile.GetUserStoreForApplication();
#else
            IsolatedStorageFile savegameStorage = IsolatedStorageFile.GetUserStoreForDomain();
#endif
            //open isolated storage, and write the savefile.
            IsolatedStorageFileStream fs = null;
            using (fs = savegameStorage.CreateFile("fish_amount"))
            {
                if (fs != null)
                {
                    //just overwrite the exsiting info for this game
                    byte[] bytes_fish = System.BitConverter.GetBytes(fishCount);
                    fs.Write(bytes_fish, 0, bytes_fish.Length);
                }
            }

            base.OnExiting(sender, args);
        }
    }
}
