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

namespace WindowsPhoneGame5
{
    /// <summary>
    /// 这是游戏的主类型
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

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

            //计算消耗时间
            float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            float totalTime = (float)gameTime.TotalGameTime.TotalSeconds;
            TouchCollection touch = new TouchCollection();

            touch = TouchPanel.GetState();

            while (TouchPanel.IsGestureAvailable)
            {
                GestureSample gs = TouchPanel.ReadGesture();
                switch (gs.GestureType)
                {
                    case GestureType.FreeDrag:
                        for (int j = 0; j < 6; j++)
                        {
                            if (!field[j].isSaw)
                            {//如果该块田未种植
                                if (gs.Position.X > 100 && gs.Position.X < 180 && gs.Position.Y > 700 && gs.Position.Y < 780)
                                {//如果手指将白菜种子所在区域按住
                                    position_01 = gs.Position;

                                    if (position_01.X > field[j].position.X && position_01.X < field[j].position.X + field[j].place.Width && position_01.Y > field[j].position.Y && position_01.Y < field[j].position.Y + field[j].place.Height)
                                    {//如果手指进行拖动并降落在某块田区域
                                        //田高亮显示
                                        field[j].color = Color.Yellow;

                                        if (gs.GestureType == GestureType.DragComplete)
                                        {//手指松开，开始种植白菜种子
                                            field[j].isSaw = true;
                                            field[j].type = 1;
                                            position_01 = new Vector2(100, 700);
                                        }//end if
                                    }//end if

                                    if (gs.GestureType == GestureType.DragComplete)
                                    {//如果没有拖到田里
                                        position_01 = new Vector2(100, 700);
                                    }//end if
                                }//end if

                                if (gs.Position.X > 380 && gs.Position.X < 460 && gs.Position.Y > 700 && gs.Position.Y < 780)
                                {//如果手指将胡萝卜种子所在区域按住
                                    position_02 = gs.Position;

                                    if (position_02.X > field[j].position.X && position_02.X < field[j].position.X + field[j].place.Width && position_02.Y > field[j].position.Y && position_02.Y < field[j].position.Y + field[j].place.Height)
                                    {//如果手指进行拖动并降落在某块田区域
                                        //田高亮显示,有空找代码吧-_-
                                        field[j].color = Color.Yellow;

                                        if (gs.GestureType == GestureType.DragComplete)
                                        {//手指松开,开始种植胡萝卜种子
                                            field[j].isSaw = true;
                                            field[j].type = 2;
                                            position_02 = new Vector2(380, 700);
                                        }//end if
                                    }//end if

                                    if (gs.GestureType == GestureType.DragComplete)
                                    {
                                        position_02 = new Vector2(380, 700);
                                    }
                                }//end if

                            }//end if
                            else
                            {//如果该田已种植
                                if (gs.Position.X > 100 && gs.Position.X < 180 && gs.Position.Y > 700 && gs.Position.Y < 780)
                                {//按住了白菜种子
                                    position_01 = gs.Position;

                                    if (gs.GestureType == GestureType.DragComplete)
                                    {
                                        position_01 = new Vector2(100, 700);
                                    }
                                }
                                if (gs.Position.X > 380 && gs.Position.X < 460 && gs.Position.Y > 700 && gs.Position.Y < 780)
                                {//如果按住了胡萝卜种子
                                    position_02 = gs.Position;

                                    if (gs.GestureType == GestureType.DragComplete)
                                    {
                                        position_02 = new Vector2(380, 700);
                                    }
                                }
                            }
                        }//end for
                        break;
                    case GestureType.Tap:
                        for (int k = 0; k < 6; k++)
                        {
                            if (gs.Position.X > 400 && gs.Position.X < 400 + harvest.Width && gs.Position.Y > 20 && gs.Position.Y < 20 + harvest.Height)
                            {//如果点按了收获按钮
                                //soundEffect.play();
                                if (field[k].isRaped)
                                {
                                    field[k].isSaw = false;
                                    field[k].isRaped = false;
                                    if (field[k].type == 1)
                                    {//收获白菜一株
                                        addChinese_cabbage();
                                        field[k].isHarvested = true;
                                    }//end if
                                    if (field[k].type == 2)
                                    {//收获胡萝卜一株
                                        addCarrot();
                                        field[k].isHarvested = true;
                                    }//end if
                                }//end if
                            }//end if
                        }//end for

                        if (gs.Position.X > 10 && gs.Position.X < 40 && gs.Position.Y > 20 && gs.Position.Y < 40)
                        {//点按返回按钮

                        }
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
            }//end forfield[j]

            //画出种子图片
            spriteBatch.Draw(seed_Chinese_cabbage, position_01, Color.Yellow);
            spriteBatch.Draw(seed_carrot, position_02, Color.Yellow);

            spriteBatch.End();
            base.Draw(gameTime);
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
