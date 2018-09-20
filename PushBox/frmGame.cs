using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace PushBox
{
    public partial class frmGame : Form
    {
        private Game game;
        private bool isAuto;
        private Thread thAuto;
        private IAuto auto;

        public frmGame()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            this.BackgroundImage = Res.OutArea;
            pnlHelp.Dock = DockStyle.Fill;
            pnlHelp.BringToFront();
            lblState.Text = string.Empty;
            lblState.BackgroundImage = Res.OutArea;
            this.FormClosing += FrmGame_FormClosing;
        }

        private void FrmGame_FormClosing(object sender, FormClosingEventArgs e)
        {
            KillAuto();
        }

        private void DoAuto(bool initFirst)
        {
            isAuto = true;
            lblState.Text = "正在自动求解，请稍后...(Alt+S取消)";
            if (initFirst)
            {
                game.InitGame();
                lblGame.Invalidate();
            }
            lblGame.Enabled = false;
            Application.DoEvents();
            thAuto = new Thread(AutoThread);
            thAuto.Start();
        }

        private void AutoThread()
        {
            if (auto == null)
            {
                auto = new AutoLessPush(s => lblState.Text = s);
            }
            var paths = auto.Run(game);
            lblGame.Enabled = true;
            Application.DoEvents();

            if (paths == null)
            {
                isAuto = false;
            }
            else
            {
                //this.Activate();
                AutoRun(paths);
            }
            isAuto = false;
            AutoNext();
            Thread.CurrentThread.Abort();
        }

        private void AutoNext()
        {
            if (game.Level < Maps.AllMap.Count)
            {
                Thread.Sleep(150);
                Application.DoEvents();
                game.LoadNextLevel();
                InPlaceGame();
                lblState.Text = string.Empty;
                Application.DoEvents();
                DoAuto(false);
            }
        }

        private void AutoRun(object obj)
        {
            var paths = obj as List<int>;
            if (paths != null)
            {
                for (int i = 0; i < paths.Count; i++)
                {
                    Application.DoEvents();
                    game.Move(paths[i]);
                    lblGame.Invalidate();
                    Application.DoEvents();
                    if (i < paths.Count - 1)
                    {
                        Thread.Sleep(50);
                    }
                }
            }
        }

        private void KillAuto()
        {
            auto?.Stop();
            thAuto?.Abort();
            isAuto = false;
            lblState.Text = string.Empty;
            lblGame.Enabled = true;
        }

        //0是空白区域，1是墙，2是箱子，3是目标位置，4是归位的箱子，5是搬运工，6是搬运工踩在目标位置上
        private void lblGame_Paint(object sender, PaintEventArgs e)
        {
            Graphics gp = e.Graphics;
            for (int i = 0; i < game.Map.Length; i++)
            {
                int x = i % game.W * 30, y = i / game.W * 30;
                switch (game.Map[i])
                {
                    case 1:
                        gp.DrawImage(Res.Wall, x, y, 30, 30);
                        break;
                    case 2:
                        gp.DrawImage(Res.Box, x, y, 30, 30);
                        break;
                    case 3:
                        gp.DrawImage(Res.Target, x, y, 30, 30);
                        break;
                    case 4:
                        gp.DrawImage(Res.BoxOnPlace, x, y, 30, 30);
                        break;
                    case 5:
                        gp.DrawImage(Res.Man, x, y, 30, 30);
                        break;
                    case 6:
                        gp.DrawImage(Res.ManOnPlace, x, y, 30, 30);
                        break;
                }
            }
            if (game.IsFinished)
            {
                gp.DrawString("完成", new Font("隶书", 78), new SolidBrush(Color.Red), 0, 50);
            }
            this.Text = string.Format("推箱子 - 第{0}关 - {1}步", game.Level, game.Step);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            game = new Game();
            InPlaceGame();
        }
        /// <summary>
        /// 将游戏区域的位置放置在窗体的最中央
        /// </summary>
        private void InPlaceGame()
        {
            lblGame.Location = new Point(12, 9);
            for (int i = 8; i > game.W; i--)
            {
                lblGame.Left += 15;
            }
            for (int i = 8; i > game.H; i--)
            {
                lblGame.Top += 15;
            }

            var size = new Size(282, 298);
            for (int i = 8; i < game.W; i++)
            {
                size.Width += 30;
            }
            for (int i = 8; i < game.H; i++)
            {
                size.Height += 30;
            }
            if (this.Size != size)
            {
                this.Size = size;
            }
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (isAuto)
            {
                if (e.KeyCode == Keys.S && ModifierKeys.HasFlag(Keys.Alt))
                {
                    KillAuto();
                }
                return;
            }
            if (!lblGame.Enabled)
            {
                return;
            }
            if (e.KeyCode == Keys.A && ModifierKeys.HasFlag(Keys.Alt))
            {
                bool initFirst = !ModifierKeys.HasFlag(Keys.Shift);
                DoAuto(initFirst);
                return;
            }
            if (pnlHelp.Visible || e.KeyCode == Keys.H)
            {
                pnlHelp.Visible = !pnlHelp.Visible;
                return;
            }
            if (pnlHelp.Visible)
            {
                this.Text = "推箱子 - 帮助";
                return;
            }
            if (e.KeyCode == Keys.PageDown || game.IsFinished && e.KeyCode == Keys.Enter)
            {
                if (game.Level < Maps.AllMap.Count)
                {
                    game.LoadNextLevel();
                    InPlaceGame();
                    lblState.Text = string.Empty;
                }
            }
            else if (e.KeyCode == Keys.PageUp)
            {
                if (game.Level > 1)
                {
                    game.LoadPrevLevel();
                    InPlaceGame();
                    lblState.Text = string.Empty;
                }
            }
            if (e.KeyCode == Keys.Enter)
            {
                game.InitGame();
                lblState.Text = string.Empty;
            }
            if (game.IsFinished)
            {
                return;
            }
            if (e.KeyCode >= Keys.Left && e.KeyCode <= Keys.Down)
            {
                game.Move((int)e.KeyCode - 37);
            }
            if (e.KeyCode == Keys.Back)
            {
                game.Back();
            }
            lblGame.Invalidate();
        }

        private void pnlHelp_Click(object sender, EventArgs e)
        {
            pnlHelp.Visible = false;
        }
    }
}