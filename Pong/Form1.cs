/*
 * Description:     A basic PONG simulator
 * Author:           
 * Date:            
 */

#region libraries

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Media;

#endregion

namespace Pong
{
    public partial class Form1 : Form
    {

        #region global values
        //random value generator
        private Random random = new Random();
        //graphics objects for drawing
        SolidBrush whiteBrush = new SolidBrush(Color.White);
        Pen whitePen = new Pen(Color.White, 4);
        Pen redPen;
        Pen bluePen;
        Pen yellowPen;
        Font drawFont = new Font("Courier New", 10);

        // Sounds for game
        SoundPlayer scoreSound = new SoundPlayer(Properties.Resources.score);
        SoundPlayer collisionSound = new SoundPlayer(Properties.Resources.collision);
        SoundPlayer paddleCollisionSound = new SoundPlayer(Properties.Resources.paddle_hit);

        //determines whether a key is being pressed or not
        Boolean wKeyDown, sKeyDown, upKeyDown, downKeyDown;

        // check to see if a new game can be started
        Boolean newGameOk = true;

        //ball values
        Boolean ballMoveRight = true;
        Boolean ballMoveDown = true;
        const int BALL_SPEED = 7;
        const int BALL_WIDTH = 20;
        const int BALL_HEIGHT = 20; 
        Rectangle ball;

        //player values
        const int PADDLE_SPEED = 6;
        const int PADDLE_EDGE = 20;  // buffer distance between screen edge and paddle            
        const int PADDLE_WIDTH = 10;
        const int PADDLE_HEIGHT = 40;
        Rectangle player1, player2;

        //player and game scores
        int player1Score = 0;
        int player2Score = 0;
        int gameWinScore = 3;  // number of points needed to win game
        #endregion

        public Form1()
        {
            InitializeComponent();
            Color transparentRed = Color.FromArgb(200, Color.Red);
            redPen = new Pen(transparentRed, 6);
            Color transparentBlue = Color.FromArgb(70, Color.Blue);
            bluePen = new Pen(transparentBlue, 5);
            Color transparentYellow = Color.FromArgb(200, Color.Yellow);
            yellowPen = new Pen(transparentYellow, 6);
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            //check to see if a key is pressed and set is KeyDown value to true if it has
            switch (e.KeyCode)
            {
                case Keys.W:
                    wKeyDown = true;
                    break;
                case Keys.S:
                    sKeyDown = true;
                    break;
                case Keys.Up:
                    upKeyDown = true;
                    break;
                case Keys.Down:
                    downKeyDown = true;
                    break;
                case Keys.Y:
                case Keys.Space:
                    if (newGameOk)
                    {
                        SetParameters();
                    }
                    break;
                case Keys.Escape:
                    if (newGameOk)
                    {
                        Close();
                    }
                    break;
            }
        }
        
        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            //check to see if a key has been released and set its KeyDown value to false if it has
            switch (e.KeyCode)
            {
                case Keys.W:
                    wKeyDown = false;
                    break;
                case Keys.S:
                    sKeyDown = false;
                    break;
                case Keys.Up:
                    upKeyDown = false;
                    break;
                case Keys.Down:
                    downKeyDown = false;
                    break;
            }
        }

        /// <summary>
        /// sets the ball and paddle positions for game start
        /// </summary>
        private void SetParameters()
        {
            if (newGameOk)
            {
                player1Score = player2Score = 0;
                newGameOk = false;
                startLabel.Visible = false;
                gameUpdateLoop.Start();
            }

            //player start positions
            player1 = new Rectangle(PADDLE_EDGE, this.Height / 2 - PADDLE_HEIGHT / 2, PADDLE_WIDTH, PADDLE_HEIGHT);
            player2 = new Rectangle(this.Width - PADDLE_EDGE - PADDLE_WIDTH, this.Height / 2 - PADDLE_HEIGHT / 2, PADDLE_WIDTH, PADDLE_HEIGHT);

            // TODO create a ball rectangle in the middle of screen
            ball = new Rectangle(this.Width / 2 - BALL_WIDTH / 2, this.Height / 2 - BALL_HEIGHT / 2, BALL_WIDTH, BALL_HEIGHT);

        }

        /// <summary>
        /// This method is the game engine loop that updates the position of all elements
        /// and checks for collisions.
        /// </summary>
        private void gameUpdateLoop_Tick(object sender, EventArgs e)
        {
            #region update ball position

            if (ballMoveRight == true)
            {
                ball.X += BALL_SPEED;
            }
            else
            {
                ball.X -= BALL_SPEED;
            }

            if (ballMoveDown == true)
            {
                ball.Y += BALL_SPEED;
            }
            else
            {
                ball.Y -= BALL_SPEED;
            }
            #endregion

            #region update paddle positions

            if (wKeyDown == true && player1.Y > 0)
            {
                player1.Y -= PADDLE_SPEED;
            }

            if (sKeyDown == true && player1.Y < this.Height - player1.Height)
            {
                player1.Y += PADDLE_SPEED;
            }

            if (upKeyDown == true && player2.Y > 0)
            {
                player2.Y -= PADDLE_SPEED;
            }
 
            if (downKeyDown == true && player2.Y < this.Height - player2.Height)
            {
                player2.Y += PADDLE_SPEED;
            }

            #endregion

            #region ball collision with top and bottom lines

            if (ball.Y < 0) // if ball hits top line
            {
                ballMoveDown = true;
                collisionSound.Play();
            }
            if (ball.Y + ball.Height > this.Height) // if ball hits bottom line
            {
                ballMoveDown = false;
                collisionSound.Play();
            }

            #endregion

            #region ball collision with paddles

            if ((ball.IntersectsWith(player1) && ball.X < this.Width / 2) || (ball.IntersectsWith(player2) && ball.X > this.Width / 2))
            {
                ballMoveRight =! ballMoveRight;
                collisionSound.Play();
            }

            /*  ENRICHMENT
             *  Instead of using two if statments as noted above see if you can create one
             *  if statement with multiple conditions to play a sound and change direction
             */
            #endregion

            #region ball collision with side walls (point scored)

            if (ball.X < 0)  // ball hits left wall logic
            {
                SetParameters();
                scoreSound.Play();
                player2Score += 1;
                plaery2ScoreLabel.Text = "" + player2Score;
                if (player2Score == gameWinScore)
                {
                    GameOver(false, true);
                }
                else
                {
                    ballMoveRight = !ballMoveRight;
                    SetParameters();
                }
            }
            if (ball.X + ball.Width > this.Width)  // ball hits left wall logic
            {
                SetParameters();
                scoreSound.Play();
                player1Score += 1;
                player1ScoreLabel.Text = "" + player1Score;

                if (player1Score == gameWinScore)
                {
                    GameOver(true, false);
                }
                else
                {
                    ballMoveRight = !ballMoveRight;
                    SetParameters();
                }
            }
            #endregion
            this.Refresh();
        }
        
        /// <summary>
        /// Displays a message for the winner when the game is over and allows the user to either select
        /// to play again or end the program
        /// </summary>
        /// <param name="winner">The player name to be shown as the winner</param>
        private void GameOver(bool player1Win, bool player2Win)
        {
            startLabel.Visible = true;
            newGameOk = true;
            gameUpdateLoop.Stop();
            if (player1Win == true)
            {
                startLabel.Text = "Player 1 Win \n Play Again? \n SPACE to start, ESC to Leave";
            }
            if (player2Win == true)
            {
                startLabel.Text = "Player 2 Win \n Play Again? \n SPACE to start, ESC to Leave";
            }
            Refresh();
        }

        private void Filter(List<PointF> filterList, PaintEventArgs e)
        {
            List<PointF> redList = new List<PointF>(filterList.Select(p => new PointF(p.X - 4, p.Y)).ToList());
            List<PointF> blueList = new List<PointF>(filterList.Select(p => new PointF(p.X + 3, p.Y)).ToList());
            List<PointF> yellowList = new List<PointF>(filterList.Select(p => new PointF(p.X - 2, p.Y)).ToList());

            e.Graphics.DrawPolygon(redPen, (redList).ToArray());
            e.Graphics.DrawPolygon(yellowPen, (yellowList).ToArray());
            e.Graphics.DrawPolygon(bluePen, (blueList).ToArray());
            e.Graphics.DrawPolygon(whitePen, filterList.ToArray());
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            List<PointF> player1Circle = new List<PointF>();
            List<PointF> player2Circle = new List<PointF>();

            player1Circle.AddRange(PlayerLight(player1));
            player2Circle.AddRange(PlayerLight(player2));

            

            e.Graphics.FillRectangle(whiteBrush, player1);
            e.Graphics.FillRectangle(whiteBrush, player2);
            e.Graphics.FillRectangle(whiteBrush, ball);
            
            
            Filter(player1Circle, e);
            Filter(player2Circle, e);
            int amountOfLines = 1;

            GraphicsPath night = new GraphicsPath();
            GraphicsPath light = new GraphicsPath();

            for (int i = 0; i < amountOfLines; i++)
            {
                int row = random.Next(0, this.Height);
            
                // Capture a row of pixels
                Bitmap bitmap = new Bitmap(this.Width, 1);
                using (Graphics graphics = Graphics.FromImage(bitmap))
                {
                    graphics.CopyFromScreen(0, row, 0, 0, new Size(this.Width, 1));
                }
            
                // Extract pixel colors into a list
                List<Color> pixels = new List<Color>();
                for (int x = 0; x < bitmap.Width; x++)
                {
                    pixels.Add(bitmap.GetPixel(x, 0));
                }
            
                // Shift the pixels by a random amount
                int shift = random.Next(10, 20) % pixels.Count;
                pixels = pixels.Skip(shift).Concat(pixels.Take(shift)).ToList();
            
                // Draw the shifted pixels on the screen
                for (int x = 0; x < pixels.Count; x++)
                {
                    e.Graphics.FillRectangle(new SolidBrush(pixels[x]), x, row, 1, 1);
                }
            }
        }

        private void Glitch_Tick(object sender, EventArgs e)
        {

        }

        

        private List<PointF> PlayerLight(Rectangle player)
        {
            PointF playerPosition = new PointF(player.X + player.Width / 2, player.Y + player.Height / 2);
            List<PointF> fList = new List<PointF>();
            double width = 2 * Math.PI;
            float length = 170;
            int totalPoints = 20;
            PointF originPoint = new PointF(playerPosition.X, playerPosition.Y + length);
            PointF startPoint = RotatePoint(originPoint, playerPosition, width / 2);
            PointF currentPoint = startPoint;
            for (int i = -1; i < totalPoints; i++)
            {
                PointF newPoint = RotatePoint(currentPoint, playerPosition, -(width) / totalPoints);
                fList.Add(newPoint);
                currentPoint = newPoint;
            }
            return fList;
        }
        private PointF RotatePoint(PointF point, PointF pivot, double radians)
        {
            var cosTheta = Math.Cos(radians);
            var sinTheta = Math.Sin(radians);

            var x = (cosTheta * (point.X - pivot.X) - sinTheta * (point.Y - pivot.Y) + pivot.X);
            var y = (sinTheta * (point.X - pivot.X) + cosTheta * (point.Y - pivot.Y) + pivot.Y);

            return new PointF((float)x, (float)y);
        }

    }
}
