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
using System.IO.IsolatedStorage;

namespace WindowsPhoneGame5
{
    /// <summary>
    /// 这是游戏的主类型
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        DateTimeOffset currentTime;

        Boolean flat_01 = false;//是否画出下方白菜种子
        Boolean flat_02 = false;//同上

        int carrot = 0;//收获的胡萝卜数目
        int Chinese_cabbage = 0;//收获的白菜数目

        class fields
        {//每块田都是一个对象
            public int id;

            public int type;//1代表白菜，2代表胡萝卜

            public Texture2D place;

            public Boolean isRaped;//成熟了
            public Boolean isSaw;//播种了
            public Boolean isHarvested;//收割了

            public Vector2 position;
            public Color color;

            public DateTimeOffset offset, localTime;
            public String oughtTime;

            public fields(int id, float x, float y, Boolean isSaw, Boolean isRaped, Boolean isHarvested)
            {
                this.id = id;
                this.position = new Vector2(x, y);
                this.isSaw = isSaw;
                this.isRaped = isRaped;
                this.isHarvested = isHarvested;
                this.color = Color.Green;
            }
        }

        fields[] field = new fields[6];

        public Texture2D solid;//泥土

        public Texture2D seed_cabbage_big;//在田地里显示的白菜种子图片
        public Texture2D seed_carrot_big;//在田地里显示的胡萝卜种子图片

        public Texture2D fruit_carrot;//在田地里显示的胡萝卜图片
        public Texture2D fruit_cabbage;//在田地里显示的白菜图片

        Texture2D backgroundTexture;//背景图片
        Texture2D harvest;//收获按钮
        Texture2D backwards;//后退按钮


        Texture2D seed_carrot;//胡萝卜种子图片
        Texture2D seed_Chinese_cabbage;//白菜种子图片

        Vector2 position_01;//白菜种子位置
        Vector2 position_02;//胡萝卜种子位置

        //int carrot_harvest;//记录收获的胡萝卜数
        //int chinese_cabbage_harvest;//记录收获的白菜数

        SpriteFont storageFont;//显示收获的白菜和胡萝卜的数目字体设置
        SoundEffect soundEffect;//声音效果

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            this.graphics.IsFullScreen = true;

            graphics.PreferredBackBufferWidth = 480;
            graphics.PreferredBackBufferHeight = 800;

            position_01 = new Vector2(100, 700);
            position_02 = new Vector2(380, 700);

            

            TouchPanel.EnabledGestures = GestureType.FreeDrag | GestureType.Tap | GestureType.DragComplete | GestureType.Hold;

            // Windows Phone 的默认帧速率为 30 fps。
            TargetElapsedTime = TimeSpan.FromTicks(333333);

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
            //打开本地存储独立空间，加载存在文件中的数据
#if WINDOWS_PHONE
            using (IsolatedStorageFile savegameStorage = IsolatedStorageFile.GetUserStoreForApplication())
#else
            using (IsolatedStorageFile savegameStorage = IsolatedStorageFile.GetUserStoreForDomain())
#endif
            {
                if (savegameStorage.FileExists("fields"))
                {
                    using (IsolatedStorageFileStream fs = savegameStorage.OpenFile("fields", System.IO.FileMode.Open))
                    {
                        if (fs != null)
                        {
                            //重载存储的最高记录
                            byte[] saveBytes_carrot = new byte[4];
                            byte[] saveBytes_cabbage = new byte[4];

                            int count_carrot = fs.Read(saveBytes_carrot, 0, 4);
                            int count_cabbage = fs.Read(saveBytes_cabbage, 0, 4);
                            if (count_carrot > 0)
                            {
                                carrot = System.BitConverter.ToInt32(saveBytes_carrot, 0);
                            }
                        }
                    }
                }
            }

            base.Initialize();
        }

        /// <summary>
        /// 对于每个游戏会调用一次 LoadContent，
        /// 用于加载所有内容。
        /// </summary>
        protected override void LoadContent()
        {
            // 创建新的 SpriteBatch，可将其用于绘制纹理。
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: 在此处使用 this.Content 加载游戏内容
            //ContentManager cm = this.Content;
            

            backgroundTexture = Content.Load<Texture2D>("background");

            storageFont = Content.Load<SpriteFont>("Times New Roman");

            field[0] = new fields(1, 50, 50, false, false, false);
            field[1] = new fields(2, 290, 50, false, false, false);
            field[2] = new fields(3, 50, 275, false, false, false);
            field[3] = new fields(4, 290, 275, false, false, false);
            field[4] = new fields(5, 50, 500, false, false, false);
            field[5] = new fields(6, 290, 500, false, false, false);

            for (int i = 0; i < 6; i++)
            {
                field[i].place = Content.Load<Texture2D>("solid");
            }

            solid = Content.Load<Texture2D>("solid");
            seed_carrot = Content.Load<Texture2D>("seed_carrot");
            seed_Chinese_cabbage = Content.Load<Texture2D>("seed_chinese_cabbage");
            fruit_carrot = Content.Load<Texture2D>("fruit_carrot");
            fruit_cabbage = Content.Load<Texture2D>("fruit_cabbage");

            //显示田地里的种子图片
            seed_cabbage_big = Content.Load<Texture2D>("seed_Chinese_cabbage_big");
            seed_carrot_big = Content.Load<Texture2D>("seed_carrot_big");

            harvest = Content.Load<Texture2D>("harvest");
            backwards = Content.Load<Texture2D>("backwards");

            soundEffect = Content.Load<SoundEffect>("Windows Ding");
        }

        /// <summary>
        /// 对于每个游戏会调用一次 UnloadContent，
        /// 用于取消加载所有内容。
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: 在此处取消加载任何非 ContentManager 内容
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
            Viewport view = graphics.GraphicsDevice.Viewport;

            //获取当前时间
            currentTime = new DateTimeOffset();

            //计算消耗时间
            float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            float totalTime = (float)gameTime.TotalGameTime.TotalSeconds;

            for (int j = 0; j < 8; j++)
            {
                //从本地存储的数据转换为内存中的数组
                //提取之前储存在本地标号为j的时间戳，转换为DateTimeOffset格式
                

                if (Equals(field[j].oughtTime, currentTime))
                {
                    field[j].isRaped = true;
                }
            }

            TouchCollection touchCollection = TouchPanel.GetState();
            foreach (TouchLocation tl in touchCollection)
            {
                if (tl.State == TouchLocationState.Pressed)
                {
                    if (tl.Position.X >= 100 && tl.Position.X <= 180 && tl.Position.Y >= 700 && tl.Position.Y <= 780 && tl.State == TouchLocationState.Released)
                    {//如果点按了白菜种子
                        flat_01 = true;

                        for (int i = 0; i < 6; i++)
                        {//对于每一块田
                            if (!field[i].isSaw)
                            {
                                field[i].color = Color.Yellow;

                                if (tl.State == TouchLocationState.Pressed)
                                {
                                    if (tl.Position.X >= field[i].position.X && tl.Position.X <= field[i].position.X + field[i].place.Width && tl.State == TouchLocationState.Released)
                                    {//按住了某一块田
                                        field[i].isSaw = true;
                                        field[i].type = 1;
                                        //获取手机系统时间开始计时60分钟
                                        field[i].offset = DateTimeOffset.UtcNow;
                                        field[i].localTime = field[i].offset.ToLocalTime();
                                        field[i].oughtTime = field[i].localTime.AddMinutes(60).ToString();
                                        //把这块田的所有最新信息(不是此时，而是退出游戏时)存到手机里，按照标号i

                                        field[i].color = Color.Green;
                                        flat_01 = false;
                                    }//end if
                                }//end if
                            }//end if
                        }//end for
                    }//end if
                    if (tl.Position.X >= 380 && tl.Position.X <= 460 && tl.Position.Y >= 700 && tl.Position.Y <= 780 && tl.State == TouchLocationState.Released)
                    {//如果点按了胡萝卜种子
                        flat_02 = true;

                        for (int i = 0; i < 6; i++)
                        {//对于每一块田
                            if (!field[i].isSaw)
                            {
                                field[i].color = Color.Yellow;

                                if (tl.State == TouchLocationState.Pressed)
                                {
                                    if (tl.Position.X >= field[i].position.X && tl.Position.X <= field[i].position.X + field[i].place.Width && tl.State == TouchLocationState.Released)
                                    {//按住了某一块田
                                        field[i].isSaw = true;
                                        field[i].type = 2;
                                        //获取手机系统时间开始计时60分钟
                                        field[i].offset = DateTimeOffset.UtcNow;
                                        field[i].localTime = field[i].offset.ToLocalTime();
                                        field[i].oughtTime = field[i].localTime.AddMinutes(60).ToString();
                                        //把这块田的所有信息存到手机里，按照标号i

                                        field[i].color = Color.Green;
                                        flat_02 = false;
                                    }//end if
                                }//end if
                            }//end if
                        }//end for
                    }//end if

                    if (tl.Position.X >= 400 && tl.Position.X <= 400 + harvest.Width && tl.Position.Y >= 20 && tl.Position.Y <= 20 + harvest.Height)
                    {//如果点按收割按钮
                        if (tl.State == TouchLocationState.Released)
                        {
                            for (int i = 0; i < 8; i++)
                            {
                                if (field[i].isRaped)
                                {
                                    if (field[i].type == 1)
                                    {//收获一株白菜
                                        addChinese_cabbage();
                                    }
                                    else if (field[i].type == 2)
                                    {//收获一根胡萝卜
                                        addCarrot();
                                    }

                                    field[i].isHarvested = true;//标记为已收获
                                    field[i].isSaw = false;//标记为未播种，可以播种
                                }//end if
                            }//end for
                        }//end if
                    }//end if
                }//end if
            }//end foreach

            base.Update(gameTime);
        }

        /// <summary>
        /// 当游戏该进行自我绘制时调用此项。
        /// </summary>
        /// <param name="gameTime">提供计时值的快照。</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: 在此处添加绘图代码
            base.Draw(gameTime);
            
            spriteBatch.Begin();

            //添加背景图片
            spriteBatch.Draw(backgroundTexture, new Rectangle(0, 0, Window.ClientBounds.Width, Window.ClientBounds.Height), null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0);

            //添加返回按钮
            spriteBatch.Draw(backwards, new Vector2(10, 20), Color.White);

            //添加数目栏
            spriteBatch.DrawString(storageFont, "Chinese_cabbage: " + Chinese_cabbage, new Vector2(80, 20), Color.Yellow, 0, Vector2.Zero, 1, SpriteEffects.None, 1);
            spriteBatch.DrawString(storageFont, "Carrot: " + carrot, new Vector2(250, 20), Color.Yellow, 0, Vector2.Zero, 1, SpriteEffects.None, 1);

            //添加收割按钮
            spriteBatch.Draw(harvest, new Vector2(410, 10), Color.White);

            //画出6块种植田
            for (int j = 0; j < 6; j++)
            {
                if (!field[j].isSaw)
                {//如果未播种
                    spriteBatch.Draw(solid, field[j].position, field[j].color);
                }
                else
                {
                    if (field[j].isSaw)
                    {//如果播种了
                        if (!field[j].isRaped)
                        {//如果播种了但没有成熟
                            if (field[j].type == 1)
                            {
                                spriteBatch.Draw(seed_cabbage_big, field[j].position, field[j].color);
                            }

                            if (field[j].type == 2)
                            {
                                spriteBatch.Draw(seed_carrot_big, field[j].position, field[j].color);
                            }

                        }
                        else
                        {//如果成熟了
                            if (field[j].type == 1)
                            {
                                spriteBatch.Draw(fruit_cabbage, field[j].position, field[j].color);
                            }
                            if (field[j].type == 2)
                            {
                                spriteBatch.Draw(fruit_carrot, field[j].position, field[j].color);
                            }
                        }
                    }
                }
            }//end foreach

            //画出种子图片
            if (flat_01)
            {
                spriteBatch.Draw(seed_Chinese_cabbage, position_01, Color.Yellow);
            }
            else
            {
                spriteBatch.Draw(seed_Chinese_cabbage, position_01, Color.Green);
            }

            if (flat_02)
            {
                spriteBatch.Draw(seed_carrot, position_02, Color.Yellow);
            }
            else
            {
                spriteBatch.Draw(seed_carrot, position_02, Color.Green);
            }

            

            spriteBatch.End();
            
        }

        protected override void OnExiting(object sender, EventArgs args)
        {
            //保存游戏状态信息，这里包括每块田的信息和收获的白数书和胡萝卜数
#if WINDOWS_PHONE
            IsolatedStorageFile savegameStorage = IsolatedStorageFile.GetUserStoreForApplication();
#else
            IsolatedStorageFile savegameStorage = IsolatedStorageFile.GetUserStoreForDomain();
#endif

            //打开独立存储空间，写文件
            IsolatedStorageFileStream fs = null;
            using (fs = savegameStorage.CreateFile("fields"))
            {
                if (fs != null)
                {
                    //写数据
                    byte[] bytes_carrot = System.BitConverter.GetBytes(carrot);//写入得到的胡萝卜数
                    byte[] bytes_Chinese_cabbage = System.BitConverter.GetBytes(Chinese_cabbage);//写入得到的白菜数

                    fs.Write(bytes_carrot, 0, bytes_carrot.Length);
                    fs.Write(bytes_Chinese_cabbage, 0, bytes_Chinese_cabbage.Length);
                }
            }

            base.OnExiting(sender, args);
        }

        public void addCarrot()
        {
            this.carrot++;
        }

        public void addChinese_cabbage()
        {
            this.Chinese_cabbage++;
        }
    }
}
