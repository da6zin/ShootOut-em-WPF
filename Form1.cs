using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Media;

namespace Shoot_Out_Game_MOO_ICT
{
    public partial class Form1 : Form
    {
        private ProgressBar speedPowerBar;
        private System.Windows.Forms.Label powerupKeyLabel;
        private int powerupCooldownCounter = 0;
        private bool isSpeedBoostReady = true;

        private PictureBox wallLeft;
        private PictureBox wallRight;
        private PictureBox wallTop;
        private PictureBox wallBottom;

        bool goLeft, goRight, goUp, goDown, gameOver;
        string facing = "up";
        int playerHealth = 100;
        int speed = 10;
        int ammo = 10;
        int zombieSpeed = 3;
        int maxZombies = 4;
        int spawnDelayCounter = 0;
        bool hasDroppedMedkit = false;
        bool hasDroppedAmmo = false;
        Random randNum = new Random();
        int score;
        List<PictureBox> zombiesList = new List<PictureBox>();

        private int playerNormalSpeed = 10;
        private int playerBoostedSpeed = 20;

        private int bossZombieHealth = 3;
        private bool isNextZombieBoss = false;

        public Form1()
        {
            InitializeComponent();

            this.speedPowerBar = new ProgressBar();
            this.speedPowerBar.Location = new Point(500, 17);
            this.speedPowerBar.Name = "speedPowerBar";
            this.speedPowerBar.Size = new Size(150, 23);
            this.speedPowerBar.Maximum = 15;
            this.Controls.Add(this.speedPowerBar);
            this.speedPowerBar.BringToFront();

            this.powerupKeyLabel = new System.Windows.Forms.Label();
            this.powerupKeyLabel.AutoSize = true;
            this.powerupKeyLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.powerupKeyLabel.ForeColor = System.Drawing.Color.White;
            this.powerupKeyLabel.Location = new System.Drawing.Point(450, 15);
            this.powerupKeyLabel.Name = "powerupKeyLabel";
            this.powerupKeyLabel.Text = "(B)";
            this.Controls.Add(this.powerupKeyLabel);
            this.powerupKeyLabel.BringToFront();

            RestartGame();
        }

        private void MainTimerEvent(object sender, EventArgs e)
        {
            this.Focus();

            if (score > 20)
            {
                maxZombies = 6;
                zombieSpeed = 5;
            }
            else if (score > 10)
            {
                maxZombies = 5;
                zombieSpeed = 4;
            }
            else
            {
                maxZombies = 4;
                zombieSpeed = 3;
            }

            if (playerHealth > 0)
            {
                healthBar.Value = playerHealth;
            }
            else
            {
                healthBar.Value = 0;
                gameOver = true;
                player.Image = Properties.Resources.dead;
                GameTimer.Stop();
            }

            if (zombiesList.Count < maxZombies && spawnDelayCounter > 0)
            {
                spawnDelayCounter--;
            }
            if (spawnDelayCounter == 0 && zombiesList.Count < maxZombies)
            {
                MakeZombies();
            }

            if (goLeft == true && player.Left > wallLeft.Right)
            {
                player.Left -= speed;
            }
            if (goRight == true && player.Left + player.Width < wallRight.Left)
            {
                player.Left += speed;
            }
            if (goUp == true && player.Top > wallTop.Bottom)
            {
                player.Top -= speed;
            }
            if (goDown == true && player.Top + player.Height < wallBottom.Top)
            {
                player.Top += speed;
            }

            if (score > 0 && score % 10 == 0 && !hasDroppedMedkit)
            {
                DropMedkit();
                hasDroppedMedkit = true;
            }
            else if (score % 10 != 0)
            {
                hasDroppedMedkit = false;
            }

            if (ammo < 2 && !hasDroppedAmmo)
            {
                DropAmmo();
                DropAmmo();
                hasDroppedAmmo = true;
            }
            else if (ammo >= 2)
            {
                hasDroppedAmmo = false;
            }

            foreach (Control x in this.Controls)
            {
                if (x is PictureBox && (string)x.Tag == "ammo")
                {
                    if (player.Bounds.IntersectsWith(x.Bounds))
                    {
                        this.Controls.Remove(x);
                        ((PictureBox)x).Dispose();
                        ammo += 5;
                    }
                }

                else if (x is PictureBox && (string)x.Tag == "medkit")
                {
                    if (player.Bounds.IntersectsWith(x.Bounds))
                    {
                        this.Controls.Remove(x);
                        ((PictureBox)x).Dispose();
                        playerHealth += 25;

                        if (playerHealth > 100)
                        {
                            playerHealth = 100;
                        }
                    }
                }

                if (x is PictureBox && (string)x.Tag == "bullet")
                {
                    foreach (Control j in this.Controls)
                    {
                        if (j is PictureBox && (string)j.Tag == "zombie")
                        {
                            if (x.Bounds.IntersectsWith(j.Bounds))
                            {
                                if (j.Size.Width == 100)
                                {
                                    bossZombieHealth--;
                                    this.Controls.Remove(x);
                                    ((PictureBox)x).Dispose();

                                    if (bossZombieHealth <= 0)
                                    {
                                        score += 3;
                                        this.Controls.Remove(j);
                                        ((PictureBox)j).Dispose();
                                        zombiesList.Remove(((PictureBox)j));
                                        bossZombieHealth = 3;
                                        spawnDelayCounter = 50;

                                        if (score > 0 && score % 15 == 0)
                                        {
                                            isNextZombieBoss = true;
                                        }
                                    }
                                }
                                else
                                {
                                    score++;
                                    this.Controls.Remove(x);
                                    ((PictureBox)x).Dispose();
                                    this.Controls.Remove(j);
                                    ((PictureBox)j).Dispose();
                                    zombiesList.Remove(((PictureBox)j));
                                    spawnDelayCounter = 50;

                                    if (score > 0 && score % 15 == 0)
                                    {
                                        isNextZombieBoss = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            foreach (PictureBox zombie in zombiesList)
            {
                if (zombie.Left > player.Left)
                {
                    zombie.Left -= zombieSpeed;
                    zombie.Image = Properties.Resources.zleft;
                }
                if (zombie.Left < player.Left)
                {
                    zombie.Left += zombieSpeed;
                    zombie.Image = Properties.Resources.zright;
                }
                if (zombie.Top > player.Top)
                {
                    zombie.Top -= zombieSpeed;
                    zombie.Image = Properties.Resources.zup;
                }
                if (zombie.Top < player.Top)
                {
                    zombie.Top += zombieSpeed;
                    zombie.Image = Properties.Resources.zdown;
                }

                if (player.Bounds.IntersectsWith(zombie.Bounds))
                {
                    playerHealth -= 1;
                }
            }

            txtAmmo.Text = "Munição: " + ammo;
            txtScore.Text = "Mortes: " + score;

            if (playerHealth >= 0)
            {
                healthBar.Value = playerHealth;
            }
            else
            {
                healthBar.Value = 0;
            }
        }

        private void PowerupChargeTimerEvent(object sender, EventArgs e)
        {
            if (powerupCooldownCounter < 15)
            {
                powerupCooldownCounter++;
                speedPowerBar.Value = powerupCooldownCounter;
            }

            if (powerupCooldownCounter >= 15)
            {
                ((System.Windows.Forms.Timer)sender).Stop();
                isSpeedBoostReady = true;
            }
        }

        private void KeyIsDown(object sender, KeyEventArgs e)
        {
            if (gameOver == true)
            {
                return;
            }

            if (e.KeyCode == Keys.B && isSpeedBoostReady)
            {
                isSpeedBoostReady = false;
                speed = playerBoostedSpeed;
                powerupCooldownCounter = 0;
                speedPowerBar.Value = 0;

                var powerupDurationTimer = new System.Windows.Forms.Timer();
                powerupDurationTimer.Interval = 5000;
                powerupDurationTimer.Tick += (a, b) =>
                {
                    speed = playerNormalSpeed;
                    powerupDurationTimer.Stop();
                    powerupDurationTimer.Dispose();

                    var chargeTimer = new System.Windows.Forms.Timer();
                    chargeTimer.Interval = 1000;
                    chargeTimer.Tick += PowerupChargeTimerEvent;
                    chargeTimer.Start();
                };
                powerupDurationTimer.Start();
            }

            if (e.KeyCode == Keys.Left)
            {
                goLeft = true;
                facing = "left";
                player.Image = Properties.Resources.left;
            }

            if (e.KeyCode == Keys.Right)
            {
                goRight = true;
                facing = "right";
                player.Image = Properties.Resources.right;
            }

            if (e.KeyCode == Keys.Up)
            {
                goUp = true;
                facing = "up";
                player.Image = Properties.Resources.up;
            }

            if (e.KeyCode == Keys.Down)
            {
                goDown = true;
                facing = "down";
                player.Image = Properties.Resources.down;
            }
        }

        private void KeyIsUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Left)
            {
                goLeft = false;
            }

            if (e.KeyCode == Keys.Right)
            {
                goRight = false;
            }

            if (e.KeyCode == Keys.Up)
            {
                goUp = false;
            }

            if (e.KeyCode == Keys.Down)
            {
                goDown = false;
            }

            if (e.KeyCode == Keys.Space && ammo > 0 && gameOver == false)
            {
                ammo--;
                ShootBullet(facing);
            }

            if (e.KeyCode == Keys.Enter && gameOver == true)
            {
                RestartGame();
            }
        }

        private void ShootBullet(string direction)
        {
            Bullet shootBullet = new Bullet();
            shootBullet.direction = direction;
            shootBullet.bulletLeft = player.Left + (player.Width / 2);
            shootBullet.bulletTop = player.Top + (player.Height / 2);
            shootBullet.MakeBullet(this, this.ClientSize.Width, this.ClientSize.Height);
        }

        private void MakeZombies()
        {
            PictureBox zombie = new PictureBox();
            zombie.Tag = "zombie";
            zombie.Image = Properties.Resources.zdown;

            int playerX = player.Left;
            int playerY = player.Top;
            int zombieX;
            int zombieY;

            if (isNextZombieBoss)
            {
                zombie.Size = new Size(100, 100);
                zombie.SizeMode = PictureBoxSizeMode.StretchImage;
                isNextZombieBoss = false;
            }
            else
            {
                zombie.SizeMode = PictureBoxSizeMode.AutoSize;
            }


            do
            {
                zombieX = randNum.Next(30, this.ClientSize.Width - 60);
                zombieY = randNum.Next(90, this.ClientSize.Height - 110);
            }
            while (Math.Abs(zombieX - playerX) < 150 && Math.Abs(zombieY - playerY) < 150);

            zombie.Left = zombieX;
            zombie.Top = zombieY;

            zombiesList.Add(zombie);
            this.Controls.Add(zombie);
            player.BringToFront();
        }

        private void DropMedkit()
        {
            PictureBox medkit = new PictureBox();
            medkit.Image = Properties.Resources.kitmedico;
            medkit.Size = new Size(50, 50);
            medkit.SizeMode = PictureBoxSizeMode.StretchImage;
            medkit.Location = new Point(randNum.Next(30, this.ClientSize.Width - 60), randNum.Next(90, this.ClientSize.Height - 110));
            medkit.Tag = "medkit";
            this.Controls.Add(medkit);
            medkit.BringToFront();
            player.BringToFront();
        }

        private void DropAmmo()
        {
            int ammoCount = 0;
            foreach (Control x in this.Controls)
            {
                if (x is PictureBox && (string)x.Tag == "ammo")
                {
                    ammoCount++;
                }
            }

            if (ammoCount < 3)
            {
                PictureBox ammo = new PictureBox();
                ammo.Image = Properties.Resources.ammo_Image;
                ammo.SizeMode = PictureBoxSizeMode.AutoSize;
                ammo.Left = randNum.Next(30, this.ClientSize.Width - ammo.Width - 30);
                ammo.Top = randNum.Next(90, this.ClientSize.Height - ammo.Height - 80);
                ammo.Tag = "ammo";
                this.Controls.Add(ammo);
                ammo.BringToFront();
                player.BringToFront();
            }
        }

        private void RestartGame()
        {
            foreach (Control x in this.Controls.OfType<PictureBox>().ToList())
            {
                if (x.Name != "player")
                {
                    this.Controls.Remove(x);
                    x.Dispose();
                }
            }

            zombiesList.Clear();

            goUp = false;
            goDown = false;
            goLeft = false;
            goRight = false;
            gameOver = false;
            isSpeedBoostReady = true;
            speedPowerBar.Value = 0;
            powerupCooldownCounter = 0;

            bossZombieHealth = 3;
            isNextZombieBoss = false;

            playerHealth = 100;
            score = 0;
            ammo = 10;
            zombieSpeed = 3;
            maxZombies = 4;
            spawnDelayCounter = 0;

            player.Image = Properties.Resources.up;
            player.Location = new Point(427, 471);

            wallTop = new PictureBox();
            wallTop.Size = new Size(this.ClientSize.Width, 30);
            wallTop.Location = new Point(0, 60);
            wallTop.Tag = "wall";
            wallTop.BackgroundImage = Properties.Resources.ParedeTijolos;
            wallTop.BackgroundImageLayout = ImageLayout.Tile;
            this.Controls.Add(wallTop);

            wallBottom = new PictureBox();
            wallBottom.Size = new Size(this.ClientSize.Width, 30);
            wallBottom.Location = new Point(0, this.ClientSize.Height - 30);
            wallBottom.Tag = "wall";
            wallBottom.BackgroundImage = Properties.Resources.ParedeTijolos;
            wallBottom.BackgroundImageLayout = ImageLayout.Tile;
            this.Controls.Add(wallBottom);

            wallLeft = new PictureBox();
            wallLeft.Size = new Size(30, this.ClientSize.Height - 60);
            wallLeft.Location = new Point(0, 60);
            wallLeft.Tag = "wall";
            wallLeft.BackgroundImage = Properties.Resources.ParedeTijolos;
            wallLeft.BackgroundImageLayout = ImageLayout.Tile;
            this.Controls.Add(wallLeft);

            wallRight = new PictureBox();
            wallRight.Size = new Size(30, this.ClientSize.Height - 60);
            wallRight.Location = new Point(this.ClientSize.Width - 30, 60);
            wallRight.Tag = "wall";
            wallRight.BackgroundImage = Properties.Resources.ParedeTijolos;
            wallRight.BackgroundImageLayout = ImageLayout.Tile;
            this.Controls.Add(wallRight);

            healthBar.BringToFront();
            txtAmmo.BringToFront();
            txtScore.BringToFront();
            label1.BringToFront();
            powerupKeyLabel.BringToFront();

            for (int i = 0; i < 4; i++)
            {
                MakeZombies();
            }

            var initialChargeTimer = new System.Windows.Forms.Timer();
            initialChargeTimer.Interval = 1000;
            initialChargeTimer.Tick += PowerupChargeTimerEvent;
            initialChargeTimer.Start();

            GameTimer.Start();
        }
    }
}