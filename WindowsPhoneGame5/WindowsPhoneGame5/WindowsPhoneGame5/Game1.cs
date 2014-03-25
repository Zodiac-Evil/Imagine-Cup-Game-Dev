using System;
using System.Text;
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

        int flat_01 = 0;//是否画出下方白菜种子
        int flat_02 = 0;//同上

        int carrot = 0;//收获的胡萝卜数目
        int Chinese_cabbage = 0;//收获的白菜数目

        public class fields
        {//每块田都是一个对象
            public int id;

            public int type;//1代表白菜，2代表胡萝卜，0代表没有

            public Texture2D place;

            public int isRipe;//成熟了
            public int isSaw;//播种了
            public int isHarvested;//收割了

            public Vector2 position;
            public Color color;

            public int tick;

            public fields(int id, float x, float y, int isSaw, int isRipe, int isHarvested)
            {
                this.id = id;
                this.position = new Vector2(x, y);
                this.isSaw = isSaw;
                this.isRipe = isRipe;
                this.isHarvested = isHarvested;
                this.tick = 0;
                this.type = 0;
                this.color = Color.White;
            }
        }

        public fields[] field = new fields[6];

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

            TouchPanel.EnabledGestures = GestureType.Tap;

            graphics.PreferredBackBufferWidth = 480;
            graphics.PreferredBackBufferHeight = 800;

            position_01 = new Vector2(80, 710);
            position_02 = new Vector2(320, 710);

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
            field[0] = new fields(1, 50, 50, 0, 0, 0);
            field[1] = new fields(3, 50, 275, 0, 0, 0);
            field[2] = new fields(2, 290, 50, 0, 0, 0);
            field[3] = new fields(4, 290, 275, 0, 0, 0);
            field[4] = new fields(5, 50, 500, 0, 0, 0);
            field[5] = new fields(6, 290, 500, 0, 0, 0);

            //打开本地存储独立空间，加载存在文件中的数据
#if WINDOWS_PHONE
            using (IsolatedStorageFile savegameStorage = IsolatedStorageFile.GetUserStoreForApplication())
#else
            using (IsolatedStorageFile savegameStorage = IsolatedStorageFile.GetUserStoreForDomain())
#endif
            {
                if (savegameStorage.FileExists("cabbage"))
                {
                    using (IsolatedStorageFileStream fs = savegameStorage.OpenFile("cabbage", System.IO.FileMode.Open, System.IO.FileAccess.Read))
                    {
                        if (fs != null)
                        {
                            //重载存储的最高记录
                            byte[] saveBytes_cabbage = new byte[4];
                            int count_cabbage = fs.Read(saveBytes_cabbage, 0, 4);
                            if (count_cabbage > 0)
                            {
                                this.Chinese_cabbage = System.BitConverter.ToInt32(saveBytes_cabbage, 0);
                            }
                        }
                    }
                }

                if (savegameStorage.FileExists("carrot"))
                {//读取独立存储空间中的胡萝卜数目
                    using (IsolatedStorageFileStream fs = savegameStorage.OpenFile("carrot", System.IO.FileMode.Open, System.IO.FileAccess.Read))
                    {
                        if (fs != null)
                        {
                            byte[] saveBytes_carrot = new byte[4];

                            int count_carrot = fs.Read(saveBytes_carrot, 0, 4);
                            if (count_carrot > 0)
                            {
                                this.carrot = System.BitConverter.ToInt32(saveBytes_carrot, 0);
                            }
                        }

                    }
                }

                for (int i = 1; i <= 6; i++)
                {
                    String str_01 = "-1".Insert(0, i.ToString());
                    if (savegameStorage.FileExists(str_01))
                    {
                        using (IsolatedStorageFileStream fs_01 = savegameStorage.OpenFile(str_01, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                        {
                            if (fs_01 != null)
                            {
                                //重载该块田的第一个属性值
                                byte[] saw = new byte[4];
                                int sa = fs_01.Read(saw, 0, 4);
                                if (sa > 0)
                                {//载入当前变量中
                                    this.field[i - 1].isSaw = System.BitConverter.ToInt32(saw, 0);
                                }
                            }
                        }
                    }
                    String str_02 = "-2".Insert(0, i.ToString());
                    if (savegameStorage.FileExists(str_02))
                    {
                        using (IsolatedStorageFileStream fs_02 = savegameStorage.OpenFile(str_02, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                        {
                            if (fs_02 != null)
                            {
                                //重载该块田的第二个属性值
                                byte[] ripe = new byte[4];
                                int ri = fs_02.Read(ripe, 0, 4);
                                if (ri > 0)
                                {//载入当前变量中
                                    this.field[i - 1].isRipe = System.BitConverter.ToInt32(ripe, 0);
                                }
                            }
                        }
                    }
                    String str_03 = "-3".Insert(0, i.ToString());
                    if (savegameStorage.FileExists(str_03))
                    {
                        using (IsolatedStorageFileStream fs_03 = savegameStorage.OpenFile(str_03, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                        {
                            if (fs_03 != null)
                            {
                                //重载该块田的第三个属性值
                                byte[] harvested = new byte[4];
                                int ha = fs_03.Read(harvested, 0, 4);
                                if (ha > 0)
                                {//载入当前变量中
                                    this.field[i - 1].isHarvested = System.BitConverter.ToInt32(harvested, 0);
                                }
                            }
                        }
                    }
                    String str_04 = "-4".Insert(0, i.ToString());
                    if (savegameStorage.FileExists(str_04))
                    {
                        using (IsolatedStorageFileStream fs_04 = savegameStorage.OpenFile(str_04, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                        {
                            if (fs_04 != null)
                            {
                                //重载该块田的第四个属性值
                                byte[] tickCount = new byte[4];
                                int ti = fs_04.Read(tickCount, 0, 4);
                                if (ti > 0)
                                {//载入当前变量中
                                    this.field[i - 1].tick = System.BitConverter.ToInt32(tickCount, 0);
                                }
                            }
                        }
                    }
                    String str_05 = "-5".Insert(0, i.ToString());
                    if (savegameStorage.FileExists(str_05))
                    {
                        using (IsolatedStorageFileStream fs_05 = savegameStorage.OpenFile(str_05, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                        {
                            if (fs_05 != null)
                            {
                                //重载该块田的第五个属性值
                                byte[] type = new byte[4];
                                int ty = fs_05.Read(type, 0, 4);
                                if (ty > 0)
                                {//载入当前变量中
                                    this.field[i - 1].type = System.BitConverter.ToInt32(type, 0);
                                }
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
            

            backgroundTexture = Content.Load<Texture2D>("background_grass");

            storageFont = Content.Load<SpriteFont>("Times New Roman");

            

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

            //计算消耗时间
            float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            float totalTime = (float)gameTime.TotalGameTime.TotalSeconds;

            for (int j = 0; j < 6; j++)
            {
                //从本地存储的数据转换为内存中的数组
                //一定要改变当前数组的值

                if (field[j].tick >= 1)
                {
                    if (field[j].tick == 30 * 60)
                    {//如果游戏运行了一分钟,暂不支持后台计时-_-!!!
                        this.field[j].tick = 0;//停止计时，已成熟
                        this.field[j].isRipe = 1;

                        //保存游戏状态信息，这里包括每块田的信息和收获的白数书和胡萝卜数
#if WINDOWS_PHONE
                        IsolatedStorageFile savegameStorage = IsolatedStorageFile.GetUserStoreForApplication();
#else
            IsolatedStorageFile savegameStorage = IsolatedStorageFile.GetUserStoreForDomain();
#endif

                        //打开独立存储空间，写文件

                        using (IsolatedStorageFileStream fs_carrot = savegameStorage.OpenFile("carrot", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_cabbage = savegameStorage.OpenFile("cabbage", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_1_1 = savegameStorage.OpenFile("1-1", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_1_2 = savegameStorage.OpenFile("1-2", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_1_3 = savegameStorage.OpenFile("1-3", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_1_4 = savegameStorage.OpenFile("1-4", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_1_5 = savegameStorage.OpenFile("1-5", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_2_1 = savegameStorage.OpenFile("2-1", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_2_2 = savegameStorage.OpenFile("2-2", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_2_3 = savegameStorage.OpenFile("2-3", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_2_4 = savegameStorage.OpenFile("2-4", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_2_5 = savegameStorage.OpenFile("2-5", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_3_1 = savegameStorage.OpenFile("3-1", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_3_2 = savegameStorage.OpenFile("3-2", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_3_3 = savegameStorage.OpenFile("3-3", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_3_4 = savegameStorage.OpenFile("3-4", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_3_5 = savegameStorage.OpenFile("3-5", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_4_1 = savegameStorage.OpenFile("4-1", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_4_2 = savegameStorage.OpenFile("4-2", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_4_3 = savegameStorage.OpenFile("4-3", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_4_4 = savegameStorage.OpenFile("4-4", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_4_5 = savegameStorage.OpenFile("4-5", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_5_1 = savegameStorage.OpenFile("5-1", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_5_2 = savegameStorage.OpenFile("5-2", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_5_3 = savegameStorage.OpenFile("5-3", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_5_4 = savegameStorage.OpenFile("5-4", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_5_5 = savegameStorage.OpenFile("5-5", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_6_1 = savegameStorage.OpenFile("6-1", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_6_2 = savegameStorage.OpenFile("6-2", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_6_3 = savegameStorage.OpenFile("6-3", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_6_4 = savegameStorage.OpenFile("6-4", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_6_5 = savegameStorage.OpenFile("6-5", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write))
                        {
                            IsolatedStorageFileStream[][] fs = new IsolatedStorageFileStream[6][];
                            fs[0] = new IsolatedStorageFileStream[] { fs_1_1, fs_1_2, fs_1_3, fs_1_4, fs_1_5 };
                            fs[1] = new IsolatedStorageFileStream[] { fs_2_1, fs_2_2, fs_2_3, fs_2_4, fs_2_5 };
                            fs[2] = new IsolatedStorageFileStream[] { fs_3_1, fs_3_2, fs_3_3, fs_3_4, fs_3_5 };
                            fs[3] = new IsolatedStorageFileStream[] { fs_4_1, fs_4_2, fs_4_3, fs_4_4, fs_4_5 };
                            fs[4] = new IsolatedStorageFileStream[] { fs_5_1, fs_5_2, fs_5_3, fs_5_4, fs_5_5 };
                            fs[5] = new IsolatedStorageFileStream[] { fs_6_1, fs_6_2, fs_6_3, fs_6_4, fs_6_5 };

                            if (fs_carrot != null)
                            {
                                //写数据
                                byte[] bytes_carrot = System.BitConverter.GetBytes(this.carrot);//写入收获的胡萝卜数
                                fs_carrot.Write(bytes_carrot, 0, bytes_carrot.Length);

                            }
                            if (fs_cabbage != null)
                            {
                                byte[] bytes_Chinese_cabbage = System.BitConverter.GetBytes(this.Chinese_cabbage);//写入收获的白菜数
                                fs_cabbage.Write(bytes_Chinese_cabbage, 0, bytes_Chinese_cabbage.Length);
                            }

                            for (int i = 0; i < 6; i++)
                            {
                                if (fs[i][0] != null && fs[i][1] != null && fs[i][2] != null && fs[i][3] != null && fs[i][4] != null)
                                {//对田地，要存储的就是5个值，1.是否播种，2.是否成熟，3.是否收割，4.播种到现在经历的时间， 5.蔬菜类型
                                    byte[] saw = System.BitConverter.GetBytes(this.field[i].isSaw);
                                    byte[] ripe = System.BitConverter.GetBytes(this.field[i].isRipe);
                                    byte[] harvested = System.BitConverter.GetBytes(this.field[i].isHarvested);
                                    byte[] tickCount = System.BitConverter.GetBytes(this.field[i].tick);
                                    byte[] type = System.BitConverter.GetBytes(this.field[i].type);

                                    fs[i][0].Write(saw, 0, saw.Length);
                                    fs[i][1].Write(ripe, 0, ripe.Length);
                                    fs[i][2].Write(harvested, 0, harvested.Length);
                                    fs[i][3].Write(tickCount, 0, tickCount.Length);
                                    fs[i][4].Write(type, 0, type.Length);
                                }
                            }
                        }
                    }
                    else if (field[j].tick < 30 * 60)
                    {
                        this.field[j].isRipe = 0;
                        this.field[j].tick++;
                    }
                }
            }

            if (flat_01 == 0 && flat_02 == 0)
            {
                for (int i = 0; i < 6; i++)
                {
                    field[i].color = Color.White;
                }
            }

            while (TouchPanel.IsGestureAvailable)
            {
                GestureSample gs = TouchPanel.ReadGesture();
                switch (gs.GestureType)
                {
                    case GestureType.Tap:
                        if (gs.Position.X >= 80 && gs.Position.X <= 160 && gs.Position.Y >= 710 && gs.Position.Y <= 790)
                        {//如果点按了白菜种子
                            if (flat_01 == 1)
                            {
                                flat_01 = 0;
                            }
                            else if (flat_01 == 0)
                            {
                                flat_01 = 1;
                                if (flat_02 == 1)
                                {//如果另一个种子亮着就将其变暗
                                    flat_02 = 0;
                                }
                                for (int i = 0; i < 6; i++)
                                {//对于每一块田
                                    if (0 == field[i].isSaw)
                                    {
                                        field[i].color = Color.Yellow;
                                    }//end if
                                }//end for
                            }
                        }//end if

                        else if (gs.Position.X >= 320 && gs.Position.X <= 400 && gs.Position.Y >= 710 && gs.Position.Y <= 790)
                        {//如果点按了胡萝卜种子
                            if (flat_02 == 1)
                            {
                                flat_02 = 0;
                            }
                            else if (flat_02 == 0)
                            {
                                flat_02 = 1;
                                if (flat_01 == 1)
                                {
                                    flat_01 = 0;
                                }
                                for (int i = 0; i < 6; i++)
                                {//对于每一块田
                                    if (0 == field[i].isSaw)
                                    {
                                        field[i].color = Color.Yellow;
                                    }//end if
                                }//end for
                            }
                        }//end if

                        for (int i = 0; i < 6; i++)
                        {
                            if (gs.Position.X >= field[i].position.X && gs.Position.X <= field[i].position.X + field[i].place.Width && gs.Position.Y >= field[i].position.Y && gs.Position.Y <= field[i].position.Y + field[i].place.Height)
                            {//按了某一块田
                                if (field[i].color == Color.Yellow)
                                {//如果该块田是高亮显示的
                                    if (flat_01 == 1)
                                    {
                                        field[i].isSaw = 1;
                                        field[i].type = 1;
                                        field[i].color = Color.White;
                                        flat_01 = 0;
                                        //开始计时
                                        field[i].tick = 1;
                                    }
                                    else if (flat_02 == 1)
                                    {
                                        field[i].isSaw = 1;
                                        field[i].type = 2;
                                        field[i].color = Color.White;
                                        flat_02 = 0;
                                        //开始计时
                                        field[i].tick = 1;
                                    }
                                }
                                else if (field[i].isRipe == 1)
                                {//如果成熟了就直接收割，也就是可以直接点按收割单块田
                                    field[i].isSaw = 0;
                                    field[i].isRipe = 0;
                                    field[i].isHarvested = 1;
                                    field[i].tick = 0;
                                    if (field[i].type == 1)
                                    {
                                        addChinese_cabbage();
                                    }
                                    else if (field[i].type == 2)
                                    {
                                        addCarrot();
                                    }
                                }

                                //保存游戏状态信息，这里包括每块田的信息和收获的白数书和胡萝卜数
#if WINDOWS_PHONE
                                IsolatedStorageFile savegameStorage = IsolatedStorageFile.GetUserStoreForApplication();
#else
            IsolatedStorageFile savegameStorage = IsolatedStorageFile.GetUserStoreForDomain();
#endif

                                //打开独立存储空间，写文件

                                using (IsolatedStorageFileStream fs_carrot = savegameStorage.OpenFile("carrot", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_cabbage = savegameStorage.OpenFile("cabbage", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_1_1 = savegameStorage.OpenFile("1-1", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_1_2 = savegameStorage.OpenFile("1-2", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_1_3 = savegameStorage.OpenFile("1-3", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_1_4 = savegameStorage.OpenFile("1-4", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_1_5 = savegameStorage.OpenFile("1-5", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_2_1 = savegameStorage.OpenFile("2-1", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_2_2 = savegameStorage.OpenFile("2-2", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_2_3 = savegameStorage.OpenFile("2-3", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_2_4 = savegameStorage.OpenFile("2-4", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_2_5 = savegameStorage.OpenFile("2-5", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_3_1 = savegameStorage.OpenFile("3-1", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_3_2 = savegameStorage.OpenFile("3-2", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_3_3 = savegameStorage.OpenFile("3-3", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_3_4 = savegameStorage.OpenFile("3-4", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_3_5 = savegameStorage.OpenFile("3-5", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_4_1 = savegameStorage.OpenFile("4-1", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_4_2 = savegameStorage.OpenFile("4-2", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_4_3 = savegameStorage.OpenFile("4-3", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_4_4 = savegameStorage.OpenFile("4-4", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_4_5 = savegameStorage.OpenFile("4-5", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_5_1 = savegameStorage.OpenFile("5-1", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_5_2 = savegameStorage.OpenFile("5-2", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_5_3 = savegameStorage.OpenFile("5-3", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_5_4 = savegameStorage.OpenFile("5-4", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_5_5 = savegameStorage.OpenFile("5-5", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_6_1 = savegameStorage.OpenFile("6-1", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_6_2 = savegameStorage.OpenFile("6-2", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_6_3 = savegameStorage.OpenFile("6-3", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_6_4 = savegameStorage.OpenFile("6-4", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_6_5 = savegameStorage.OpenFile("6-5", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write))
                                {
                                    IsolatedStorageFileStream[][] fs = new IsolatedStorageFileStream[6][];
                                    fs[0] = new IsolatedStorageFileStream[] { fs_1_1, fs_1_2, fs_1_3, fs_1_4, fs_1_5 };
                                    fs[1] = new IsolatedStorageFileStream[] { fs_2_1, fs_2_2, fs_2_3, fs_2_4, fs_2_5 };
                                    fs[2] = new IsolatedStorageFileStream[] { fs_3_1, fs_3_2, fs_3_3, fs_3_4, fs_3_5 };
                                    fs[3] = new IsolatedStorageFileStream[] { fs_4_1, fs_4_2, fs_4_3, fs_4_4, fs_4_5 };
                                    fs[4] = new IsolatedStorageFileStream[] { fs_5_1, fs_5_2, fs_5_3, fs_5_4, fs_5_5 };
                                    fs[5] = new IsolatedStorageFileStream[] { fs_6_1, fs_6_2, fs_6_3, fs_6_4, fs_6_5 };

                                    if (fs_carrot != null)
                                    {
                                        //写数据
                                        byte[] bytes_carrot = System.BitConverter.GetBytes(this.carrot);//写入收获的胡萝卜数
                                        fs_carrot.Write(bytes_carrot, 0, bytes_carrot.Length);

                                    }
                                    if (fs_cabbage != null)
                                    {
                                        byte[] bytes_Chinese_cabbage = System.BitConverter.GetBytes(this.Chinese_cabbage);//写入收获的白菜数
                                        fs_cabbage.Write(bytes_Chinese_cabbage, 0, bytes_Chinese_cabbage.Length);
                                    }

                                    for (int j = 0; j < 6; j++)
                                    {
                                        if (fs[j][0] != null && fs[j][1] != null && fs[j][2] != null && fs[j][3] != null && fs[j][4] != null)
                                        {//对田地，要存储的就是5个值，1.是否播种，2.是否成熟，3.是否收割，4.播种到现在经历的时间， 5.蔬菜类型
                                            byte[] saw = System.BitConverter.GetBytes(this.field[j].isSaw);
                                            byte[] ripe = System.BitConverter.GetBytes(this.field[j].isRipe);
                                            byte[] harvested = System.BitConverter.GetBytes(this.field[j].isHarvested);
                                            byte[] tickCount = System.BitConverter.GetBytes(this.field[j].tick);
                                            byte[] type = System.BitConverter.GetBytes(this.field[j].type);

                                            fs[j][0].Write(saw, 0, saw.Length);
                                            fs[j][1].Write(ripe, 0, ripe.Length);
                                            fs[j][2].Write(harvested, 0, harvested.Length);
                                            fs[j][3].Write(tickCount, 0, tickCount.Length);
                                            fs[j][4].Write(type, 0, type.Length);
                                        }
                                    }
                                }
                            }//end if
                        }

                        if (gs.Position.X >= 400 && gs.Position.X <= 400 + harvest.Width && gs.Position.Y >= 20 && gs.Position.Y <= 20 + harvest.Height)
                        {//如果点按收割按钮
                            for (int i = 0; i < 6; i++)
                            {
                                if (1 == field[i].isRipe)
                                {
                                    if (field[i].type == 1)
                                    {//收获一株白菜
                                        addChinese_cabbage();
                                    }
                                    else if (field[i].type == 2)
                                    {//收获一根胡萝卜
                                        addCarrot();
                                    }
                                    field[i].isHarvested = 1;//标记为已收获
                                    field[i].isSaw = 0;//标记为未播种，可以播种
                                    field[i].isRipe = 0;//标记为未成熟
                                    field[i].type = 0;
                                    field[i].tick = 0;//不计时
                                }//end if
                            }

                            //保存游戏状态信息，这里包括每块田的信息和收获的白数书和胡萝卜数
#if WINDOWS_PHONE
                            IsolatedStorageFile savegameStorage = IsolatedStorageFile.GetUserStoreForApplication();
#else
            IsolatedStorageFile savegameStorage = IsolatedStorageFile.GetUserStoreForDomain();
#endif

                            //打开独立存储空间，写文件

                            using (IsolatedStorageFileStream fs_carrot = savegameStorage.OpenFile("carrot", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_cabbage = savegameStorage.OpenFile("cabbage", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_1_1 = savegameStorage.OpenFile("1-1", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_1_2 = savegameStorage.OpenFile("1-2", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_1_3 = savegameStorage.OpenFile("1-3", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_1_4 = savegameStorage.OpenFile("1-4", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_1_5 = savegameStorage.OpenFile("1-5", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_2_1 = savegameStorage.OpenFile("2-1", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_2_2 = savegameStorage.OpenFile("2-2", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_2_3 = savegameStorage.OpenFile("2-3", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_2_4 = savegameStorage.OpenFile("2-4", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_2_5 = savegameStorage.OpenFile("2-5", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_3_1 = savegameStorage.OpenFile("3-1", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_3_2 = savegameStorage.OpenFile("3-2", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_3_3 = savegameStorage.OpenFile("3-3", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_3_4 = savegameStorage.OpenFile("3-4", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_3_5 = savegameStorage.OpenFile("3-5", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_4_1 = savegameStorage.OpenFile("4-1", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_4_2 = savegameStorage.OpenFile("4-2", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_4_3 = savegameStorage.OpenFile("4-3", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_4_4 = savegameStorage.OpenFile("4-4", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_4_5 = savegameStorage.OpenFile("4-5", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_5_1 = savegameStorage.OpenFile("5-1", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_5_2 = savegameStorage.OpenFile("5-2", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_5_3 = savegameStorage.OpenFile("5-3", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_5_4 = savegameStorage.OpenFile("5-4", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_5_5 = savegameStorage.OpenFile("5-5", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_6_1 = savegameStorage.OpenFile("6-1", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_6_2 = savegameStorage.OpenFile("6-2", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_6_3 = savegameStorage.OpenFile("6-3", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_6_4 = savegameStorage.OpenFile("6-4", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_6_5 = savegameStorage.OpenFile("6-5", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write))
                            {
                                IsolatedStorageFileStream[][] fs = new IsolatedStorageFileStream[6][];
                                fs[0] = new IsolatedStorageFileStream[] { fs_1_1, fs_1_2, fs_1_3, fs_1_4, fs_1_5 };
                                fs[1] = new IsolatedStorageFileStream[] { fs_2_1, fs_2_2, fs_2_3, fs_2_4, fs_2_5 };
                                fs[2] = new IsolatedStorageFileStream[] { fs_3_1, fs_3_2, fs_3_3, fs_3_4, fs_3_5 };
                                fs[3] = new IsolatedStorageFileStream[] { fs_4_1, fs_4_2, fs_4_3, fs_4_4, fs_4_5 };
                                fs[4] = new IsolatedStorageFileStream[] { fs_5_1, fs_5_2, fs_5_3, fs_5_4, fs_5_5 };
                                fs[5] = new IsolatedStorageFileStream[] { fs_6_1, fs_6_2, fs_6_3, fs_6_4, fs_6_5 };

                                if (fs_carrot != null)
                                {
                                    //写数据
                                    byte[] bytes_carrot = System.BitConverter.GetBytes(this.carrot);//写入收获的胡萝卜数
                                    fs_carrot.Write(bytes_carrot, 0, bytes_carrot.Length);

                                }
                                if (fs_cabbage != null)
                                {
                                    byte[] bytes_Chinese_cabbage = System.BitConverter.GetBytes(this.Chinese_cabbage);//写入收获的白菜数
                                    fs_cabbage.Write(bytes_Chinese_cabbage, 0, bytes_Chinese_cabbage.Length);
                                }

                                for (int j = 0; j < 6; j++)
                                {
                                    if (fs[j][0] != null && fs[j][1] != null && fs[j][2] != null && fs[j][3] != null && fs[j][4] != null)
                                    {//对田地，要存储的就是5个值，1.是否播种，2.是否成熟，3.是否收割，4.播种到现在经历的时间， 5.蔬菜类型
                                        byte[] saw = System.BitConverter.GetBytes(this.field[j].isSaw);
                                        byte[] ripe = System.BitConverter.GetBytes(this.field[j].isRipe);
                                        byte[] harvested = System.BitConverter.GetBytes(this.field[j].isHarvested);
                                        byte[] tickCount = System.BitConverter.GetBytes(this.field[j].tick);
                                        byte[] type = System.BitConverter.GetBytes(this.field[j].type);

                                        fs[j][0].Write(saw, 0, saw.Length);
                                        fs[j][1].Write(ripe, 0, ripe.Length);
                                        fs[j][2].Write(harvested, 0, harvested.Length);
                                        fs[j][3].Write(tickCount, 0, tickCount.Length);
                                        fs[j][4].Write(type, 0, type.Length);
                                    }
                                }
                            }
                        }//end if
                        break;
                }
            }

            

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
            
            
            spriteBatch.Begin();

            //添加背景图片
            spriteBatch.Draw(backgroundTexture, new Rectangle(0, 0, Window.ClientBounds.Width, Window.ClientBounds.Height), null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0);

            //添加返回按钮
            spriteBatch.Draw(backwards, new Vector2(10, 20), Color.White);

            //添加数目栏
            spriteBatch.DrawString(storageFont, "Chinese_cabbage: " + Chinese_cabbage, new Vector2(50, 20), Color.Red, 0, Vector2.Zero, 1, SpriteEffects.None, 1);
            spriteBatch.DrawString(storageFont, "Carrot: " + carrot, new Vector2(260, 20), Color.Red, 0, Vector2.Zero, 1, SpriteEffects.None, 1);

            //添加收割按钮
            spriteBatch.Draw(harvest, new Vector2(400, 6), Color.White);

            //画出6块种植田
            for (int j = 0; j < 6; j++)
            {
                if (field[j].isSaw == 0)
                {//如果未播种
                    spriteBatch.Draw(solid, field[j].position, field[j].color);
                }
                else
                {
                    if (field[j].isSaw == 1)
                    {//如果播种了
                        if (field[j].isRipe == 0)
                        {//如果播种了但没有成熟
                            if (field[j].type == 1)
                            {
                                spriteBatch.Draw(seed_cabbage_big, field[j].position, field[j].color);
                            }

                            if (field[j].type == 2)
                            {
                                spriteBatch.Draw(seed_carrot_big, field[j].position, field[j].color);
                            }
                            if (field[j].type == 0)
                            {
                                spriteBatch.Draw(solid, field[j].position, field[j].color);
                            }
                        }
                        else if (field[j].isRipe == 1)
                        {
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
            }//end for

            //画出种子图片
            if (flat_01 == 1)
            {
                spriteBatch.Draw(seed_Chinese_cabbage, position_01, Color.Yellow);
            }
            else
            {
                spriteBatch.Draw(seed_Chinese_cabbage, position_01, Color.White);
            }

            if (flat_02 == 1)
            {
                spriteBatch.Draw(seed_carrot, position_02, Color.Yellow);
            }
            else
            {
                spriteBatch.Draw(seed_carrot, position_02, Color.White);
            }

            

            spriteBatch.End();
            base.Draw(gameTime);
            
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

            using (IsolatedStorageFileStream fs_carrot = savegameStorage.OpenFile("carrot", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_cabbage = savegameStorage.OpenFile("cabbage", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_1_1 = savegameStorage.OpenFile("1-1", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_1_2 = savegameStorage.OpenFile("1-2", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_1_3 = savegameStorage.OpenFile("1-3", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_1_4 = savegameStorage.OpenFile("1-4", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_1_5 = savegameStorage.OpenFile("1-5", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_2_1 = savegameStorage.OpenFile("2-1", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_2_2 = savegameStorage.OpenFile("2-2", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_2_3 = savegameStorage.OpenFile("2-3", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_2_4 = savegameStorage.OpenFile("2-4", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_2_5 = savegameStorage.OpenFile("2-5", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_3_1 = savegameStorage.OpenFile("3-1", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_3_2 = savegameStorage.OpenFile("3-2", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_3_3 = savegameStorage.OpenFile("3-3", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_3_4 = savegameStorage.OpenFile("3-4", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_3_5 = savegameStorage.OpenFile("3-5", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_4_1 = savegameStorage.OpenFile("4-1", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_4_2 = savegameStorage.OpenFile("4-2", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_4_3 = savegameStorage.OpenFile("4-3", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_4_4 = savegameStorage.OpenFile("4-4", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_4_5 = savegameStorage.OpenFile("4-5", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_5_1 = savegameStorage.OpenFile("5-1", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_5_2 = savegameStorage.OpenFile("5-2", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_5_3 = savegameStorage.OpenFile("5-3", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_5_4 = savegameStorage.OpenFile("5-4", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_5_5 = savegameStorage.OpenFile("5-5", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_6_1 = savegameStorage.OpenFile("6-1", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_6_2 = savegameStorage.OpenFile("6-2", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_6_3 = savegameStorage.OpenFile("6-3", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_6_4 = savegameStorage.OpenFile("6-4", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), fs_6_5 = savegameStorage.OpenFile("6-5", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write))
            {
                IsolatedStorageFileStream[][] fs = new IsolatedStorageFileStream[6][];
                fs[0] = new IsolatedStorageFileStream[] { fs_1_1, fs_1_2, fs_1_3, fs_1_4, fs_1_5 };
                fs[1] = new IsolatedStorageFileStream[] { fs_2_1, fs_2_2, fs_2_3, fs_2_4, fs_2_5 };
                fs[2] = new IsolatedStorageFileStream[] { fs_3_1, fs_3_2, fs_3_3, fs_3_4, fs_3_5 };
                fs[3] = new IsolatedStorageFileStream[] { fs_4_1, fs_4_2, fs_4_3, fs_4_4, fs_4_5 };
                fs[4] = new IsolatedStorageFileStream[] { fs_5_1, fs_5_2, fs_5_3, fs_5_4, fs_5_5 };
                fs[5] = new IsolatedStorageFileStream[] { fs_6_1, fs_6_2, fs_6_3, fs_6_4, fs_6_5 };

                if (fs_carrot != null)
                {
                    //写数据
                    byte[] bytes_carrot = System.BitConverter.GetBytes(this.carrot);//写入收获的胡萝卜数
                    fs_carrot.Write(bytes_carrot, 0, bytes_carrot.Length);

                }
                if (fs_cabbage != null)
                {
                    byte[] bytes_Chinese_cabbage = System.BitConverter.GetBytes(this.Chinese_cabbage);//写入收获的白菜数
                    fs_cabbage.Write(bytes_Chinese_cabbage, 0, bytes_Chinese_cabbage.Length);
                }

                for (int i = 0; i < 6; i++)
                {
                    if (fs[i][0] != null && fs[i][1] != null && fs[i][2] != null && fs[i][3] != null &&　fs[i][4] != null)
                    {//对田地，要存储的就是5个值，1.是否播种，2.是否成熟，3.是否收割，4.播种到现在经历的时间， 5.蔬菜类型
                        byte[] saw = System.BitConverter.GetBytes(this.field[i].isSaw);
                        byte[] ripe = System.BitConverter.GetBytes(this.field[i].isRipe);
                        byte[] harvested = System.BitConverter.GetBytes(this.field[i].isHarvested);
                        byte[] tickCount = System.BitConverter.GetBytes(this.field[i].tick);
                        byte[] type = System.BitConverter.GetBytes(this.field[i].type);

                        fs[i][0].Write(saw, 0, saw.Length);
                        fs[i][1].Write(ripe, 0, ripe.Length);
                        fs[i][2].Write(harvested, 0, harvested.Length);
                        fs[i][3].Write(tickCount, 0, tickCount.Length);
                        fs[i][4].Write(type, 0, type.Length);
                    }
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