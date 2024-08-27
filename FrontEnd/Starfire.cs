using Guna.UI2.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Memory;
using static KC__LID_EXT.BackEnd.Dump.SDK;
using System.Globalization;
using STARFIRE.Backend;

namespace STARFIRE.FrontEnd
{
    public partial class Starfire : Form
    {
        #region Starfire Base
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        // Create an instance of the Mem class for memory operations
        Mem M = new Mem();
        // Boolean flag to check if the process is open
        bool IsProcOpen;
        // Variable to store the Process ID
        int ProcessID;
        // Constants for the dimensions of different parts of the trainer UI
        int TrainerWidth = 364;
        int CheatsTrainerHeight = 576;
        int PlayerTrainerHeight = 200;
        int OtherTrainerHeight = 198;
        int TeleportElevatorHeight = 230;
        int TeleportEscalatorHeight = 350;
        int TeleportResourcesHeight = 708;
        int TeleportOtherHeight = 230;
        // Variable to keep track of the number of Jackals spawned
        private int mJackalSpawnNum = 0;
        // Constant for the maximum number of Jackals that can be spawned
        private const int MaxJackalSpawnNum = 3;
        // Variable to store the spawn time for Jackals
        private float mJackalSpawnTime = 1.0f;

        public Starfire()
        {
            InitializeComponent();
            Starfire_BGWorker.RunWorkerAsync();
            // Initialize the form, then set it's state to Normal so handheld devices work.
            this.Width = TrainerWidth;
            this.Height = CheatsTrainerHeight;
            this.TopMost = true;
            this.WindowState = FormWindowState.Normal;
            Starfire_Status.Text = "STATUS: N/A";
            SubscribeToKeyPressesForHotkeys();

            // Call CheckFormPosition in the form's constructor
            CheckFormPosition();

        }
        #endregion

        #region Hotkeys
        private void GlobalHookKeyDown()
        {

            if (HotKeys.IsKeyPressed(Keys.NumPad1))
            {
                Escalator_1_Button_Click(this, EventArgs.Empty);
            }

            if (HotKeys.IsKeyPressed(Keys.NumPad2))
            {
                Escalator_2_Button_Click(this, EventArgs.Empty);
            }

            if (HotKeys.IsKeyPressed(Keys.NumPad3))
            {
                Escalator_3_Button_Click(this, EventArgs.Empty);
            }
            if (HotKeys.IsKeyPressed(Keys.NumPad4))
            {
                Resource1_Teleport_Click(this, EventArgs.Empty);
            }
            
            if (HotKeys.IsKeyPressed(Keys.NumPad5))
            {
                Resource2_Teleport_Click(this, EventArgs.Empty);
            }
            
            if (HotKeys.IsKeyPressed(Keys.NumPad6))
            {
                Resource3_Teleport_Click(this, EventArgs.Empty);
            }

            if (HotKeys.IsKeyPressed(Keys.NumPad7))
            {
                Resource4_Teleport_Click(this, EventArgs.Empty);
            }

            if (HotKeys.IsKeyPressed(Keys.NumPad8))
            {
                Resource5_Teleport_Click(this, EventArgs.Empty);
            }

            if (HotKeys.IsKeyPressed(Keys.NumPad9))
            {
                Resource6_Teleport_Click(this, EventArgs.Empty);
            }

            if (HotKeys.IsKeyPressed(Keys.NumPad0))
            {
                Resource7_Teleport_Click(this, EventArgs.Empty);
            }

            if (HotKeys.IsKeyPressed(Keys.Decimal))
            {
                Resource8_Teleport_Click(this, EventArgs.Empty);
            }
        }
        #endregion

        #region EXIT BUTTON
        // This method is called when the form is closing.
        private void Starfire_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Check if the background worker is still running
            if (Starfire_BGWorker.IsBusy)
            {
                // Request cancellation of the background worker's operation
                Starfire_BGWorker.CancelAsync();
            }
        }
        #endregion

        #region TrainerUI
        private void Starfire_Load(object sender, EventArgs e)
        {
            // Set the form window state to normal again to insure it is the correct size while on PC or any other device.
            this.WindowState = FormWindowState.Normal;
            Starfire_Status.Text = "STATUS: N/A";

            // Call CheckFormPosition when the form loads
            CheckFormPosition();
        }

        private void positionCheckTimer_Tick(object sender, EventArgs e)
        {
            // Call CheckFormPosition when the form loads
            CheckFormPosition();
        }

        private void CheckFormPosition()
        {
            // Get the screen that contains the form
            Screen screen = Screen.FromControl(this);

            // Get the bounds of the screen
            Rectangle screenBounds = screen.WorkingArea;

            // Check if the bottom of the form is out of the screen bounds
            bool isBottomOutOfScreen = this.Bottom > screenBounds.Bottom;
            // Check if the top of the form is out of the screen bounds
            bool isTopOutOfScreen = this.Top < screenBounds.Top;

            if (isBottomOutOfScreen)
            {
                // Adjust the form's position so the bottom is at the bottom edge of the screen
                this.Location = new Point(
                    this.Left, // Keep the current horizontal position
                    screenBounds.Bottom - this.Height // Set the bottom edge of the form to the bottom of the screen
                );
            }
            else if (isTopOutOfScreen)
            {
                // Adjust the form's position so the top is at the top edge of the screen
                this.Location = new Point(
                    this.Left, // Keep the current horizontal position
                    screenBounds.Top // Set the top edge of the form to the top of the screen
                );
            }
        }
        #endregion

        #region BackgroundWorker
        private void Starfire_BGWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            // Continuously check the status of the game process until the BackgroundWorker is canceled
            while (!Starfire_BGWorker.CancellationPending)
            {
                // Attempt to get the process ID of the game
                ProcessID = M.GetProcIdFromName("BrgGame-Steam.exe");
                if (ProcessID != 0)
                {
                    // If the process ID is found, open the process and set IsProcOpen to true
                    IsProcOpen = M.OpenProcess(ProcessID);
                }
                else
                {
                    // If the process ID is not found, set IsProcOpen to false
                    IsProcOpen = false;
                }

                //  check hotkey events
                GlobalHookKeyDown();

                // Sleep for a short duration to avoid CPU overuse (1000 = 1 minute)
                Thread.Sleep(1);
                // Report progress to trigger the ProcessChanged event
                Starfire_BGWorker.ReportProgress(0);
            }
            // Mark the Background Worker (BGWorker) as cancelled
            e.Cancel = true;
        }

        // This method is called when the background worker reports progress.
        private void Starfire_BGWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            // Check if the game process is open
            if (IsProcOpen)
            {
                // Update the status text to indicate the game is found
                Starfire_Status.Text = "STATUS: GAME FOUND!";

                // Read the attack up scale value from the specified memory address
                float pAtkUpScale = M.ReadFloat(UBrgUIManager.ABrgCommonPawn_CustomChara.mBaseAtkUpScale);
                // Read the attack up scale value from the specified memory address

                // Update the attack up scale value in the UI
                AtkUpScale_Value.Text = "#" + pAtkUpScale.ToString(CultureInfo.InvariantCulture);

                // Read the player time scale value from the specified memory address
                float ptimescalevalue = M.ReadFloat(UBrgUIManager.ABrgPawn_BaseNative.mDefaultTimeScale);

                // Update the player time scale value in the UI
                PlayerTimescale_Value.Text = "#" + ptimescalevalue.ToString(CultureInfo.InvariantCulture);

                // Read the player time scale value from the specified memory address
                float worldtimescalev = M.ReadFloat(UBrgUIManager.ABrgGameInfoNative.UBrgWorldTimeScaleManager.mDefaultTimeScale);

                // Update the player time scale value in the UI
                WorldTimeScale_Value.Text = "#" + worldtimescalev.ToString(CultureInfo.InvariantCulture);
            }
            else
            {
                // Update the status text to indicate the game is not found
                Starfire_Status.Text = "STATUS: N/A";
            }
        }

        // This method is called when the background worker completes its task.
        private void Starfire_BGWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // Re-run the background worker asynchronously.
            // This creates a loop where the background worker keeps running its task continuously.
            Starfire_BGWorker.RunWorkerAsync();
        }

        #endregion

        #region TabControl UI Size Changes
        private void Starfire_TabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Set the desired width and height per Tab Selected inside the trainer (CHEATS|EDITOR|TELEPORTS|OTHER)
            if (Starfire_TabControl.SelectedTab == Starfire_CheatsTab)
            {
                this.Width = TrainerWidth;
                this.Height = CheatsTrainerHeight;
            }
            else if (Starfire_TabControl.SelectedTab == Starfire_PlayerEditor)
            {
                this.Width = TrainerWidth;
                this.Height = PlayerTrainerHeight;
            }
            else if (Starfire_TabControl.SelectedTab == Starfire_Teleports)
            {
                this.Width = TrainerWidth;

                // Check which sub-tab is currently selected in the TELEPORTS tab
                if (Starfire_Teleports_TabControl.SelectedTab == ElevatorsTab)
                {
                    this.Height = TeleportElevatorHeight;
                }
                else if (Starfire_Teleports_TabControl.SelectedTab == EscalatorsTab)
                {
                    this.Height = TeleportEscalatorHeight;
                }
                else if (Starfire_Teleports_TabControl.SelectedTab == ResourcesTab)
                {
                    this.Height = TeleportResourcesHeight;
                }
                else if (Starfire_Teleports_TabControl.SelectedTab == OtherTab)
                {
                    this.Height = TeleportOtherHeight;
                }
            }
            else if (Starfire_TabControl.SelectedTab == Starfire_Other)
            {
                this.Width = TrainerWidth;
                this.Height = OtherTrainerHeight;
            }
        }
        private void Starfire_Teleports_TabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Set the desired width and height per Tab Selected inside the trainer (ELEVATORS|ESCALATORS|OTHER)
            if (Starfire_Teleports_TabControl.SelectedTab == ElevatorsTab)
            {
                this.Width = TrainerWidth;
                this.Height = TeleportElevatorHeight;
            }
            else if (Starfire_Teleports_TabControl.SelectedTab == EscalatorsTab)
            {
                this.Width = TrainerWidth;
                this.Height = TeleportEscalatorHeight;
            }
            else if (Starfire_Teleports_TabControl.SelectedTab == ResourcesTab)
            {
                this.Width = TrainerWidth;
                this.Height = TeleportResourcesHeight;
            }
            else if (Starfire_Teleports_TabControl.SelectedTab == OtherTab)
            {
                this.Width = TrainerWidth;
                this.Height = TeleportOtherHeight;
            }
        }
        private void Starfire_Exit_Click_1(object sender, EventArgs e)
        {
            // Check if the game is currently running
            if (Starfire_Status.Text == "STATUS: GAME FOUND!")
            {
                // Disable all toggles to ensure proper cleanup
                if (Invincible_Toggle.Checked) Invincible_Toggle.Checked = false;
                if (NoDamage_Toggle.Checked) NoDamage_Toggle.Checked = false;
                if (InfiniteHP_Toggle.Checked) InfiniteHP_Toggle.Checked = false;
                if (InfiniteStamina_Toggle.Checked) InfiniteStamina_Toggle.Checked = false;
                if (InfiniteRage_Toggle.Checked) InfiniteRage_Toggle.Checked = false;
                if (InfiniteDurability_Toggle.Checked) InfiniteDurability_Toggle.Checked = false;
                if (SpeedhackWalk_Toggle.Checked) SpeedhackWalk_Toggle.Checked = false;
                if (SpeedhackSprint_Toggle.Checked) SpeedhackSprint_Toggle.Checked = false;
                if (SuperJump_Toggle.Checked) SuperJump_Toggle.Checked = false;
                if (OneHitKill_Toggle.Checked) OneHitKill_Toggle.Checked = false;
                if (NoRecoil_Toggle.Checked) NoRecoil_Toggle.Checked = false;
                if (InfiniteAmmo_Toggle.Checked) InfiniteAmmo_Toggle.Checked = false;
                if (TPose_Toggle.Checked) TPose_Toggle.Checked = false;
                if (NoFallDamage_Toggle.Checked) NoFallDamage_Toggle.Checked = false;
                if (UnFogMiniMap_Toggle.Checked) UnFogMiniMap_Toggle.Checked = false;
                if (KillcoinVacuum_Toggle.Checked) KillcoinVacuum_Toggle.Checked = false;
                if (OpenDailyRewardBox_Toggle.Checked) OpenDailyRewardBox_Toggle.Checked = false;
                if (InstantSpawnJackals_Toggle.Checked) InstantSpawnJackals_Toggle.Checked = false;
                if (AtkUpScale_Toggle.Checked) AtkUpScale_Toggle.Checked = false;
                if (PlayerTimescale_Toogle.Checked) PlayerTimescale_Toogle.Checked = false;
                if (WorldTimeScale_Toggle.Checked) WorldTimeScale_Toggle.Checked = false;

                // Reset the values to 300 when exiting the application
                M.WriteMemory(UBrgUIManager.ABrgGameInfoNative.UBrgDatabaseParam.mJackalSpawnStartTime, "float", "300");
                M.WriteMemory(UBrgUIManager.ABrgGameInfoNative.UBrgDatabaseParam.mJackalAwakeSpawnStartTime, "float", "300");
                M.WriteMemory(UBrgUIManager.ABrgGameInfoNative.UBrgDatabaseParam.mJackalRebornSpawnStartTime, "float", "300");

                // Delay to allow all toggles to be processed before exit
                Task.Delay(1000);
                Application.Exit();
            }
            // Check if the game is not running, if not running exit program
            else if (Starfire_Status.Text == "STATUS: N/A")
            {
                // Terminate the execution of the current application and exit with a success code (0).
                Application.Exit();
            }
        }
        #endregion

        #region CheatsTab
        private void Invincible_Toggle_CheckedChanged(object sender, EventArgs e)
        {
            // Check if the game is currently running
            if (Starfire_Status.Text == "STATUS: GAME FOUND!")
            {
                if (Invincible_Toggle.Checked)
                {
                    // Enable Invincible when the toggle is checked
                    Invincible_Label.Text = "- INVINCIBLE: ENABLED";
                    try
                    {
                        M.WriteMemory(UBrgUIManager.ABrgPawn_BaseNative.mInvincible, "int", "1");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error enabling Invincible: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Invincible_Toggle.Checked = false;
                    }
                }
                else
                {
                    // Disable Invincible when the toggle is unchecked
                    Invincible_Label.Text = "- INVINCIBLE: DISABLED";
                    try
                    {
                        M.WriteMemory(UBrgUIManager.ABrgPawn_BaseNative.mInvincible, "int", "0");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error disabling Invincible: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            // Check if the game is not running
            else if (Starfire_Status.Text == "STATUS: N/A")
            {
                if (Invincible_Toggle.Checked)
                {
                    // Force disable the feature if the game is not running
                    Invincible_Toggle.Checked = false;
                    Invincible_Label.Text = "- INVINCIBLE: DISABLED";
                }
            }
            // Optional: Handle any other unexpected status
            else
            {
                MessageBox.Show("Unexpected game status. Please check the game state.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

        }

        private void NoDamage_Toggle_CheckedChanged(object sender, EventArgs e)
        {
            // Check if the game is currently running
            if (Starfire_Status.Text == "STATUS: GAME FOUND!")
            {
                if (NoDamage_Toggle.Checked)
                {
                    // Enable No Damage when the toggle is checked
                    NoDamage_Label.Text = "- NO DAMAGE: ENABLED";
                    try
                    {
                        M.WriteMemory(UBrgUIManager.ABrgPawn_BaseNative.mNoDamage, "int", "1");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error enabling No Damage: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        NoDamage_Toggle.Checked = false;
                    }
                }
                else
                {
                    // Disable No Damage when the toggle is unchecked
                    NoDamage_Label.Text = "- NO DAMAGE: DISABLED";
                    try
                    {
                        M.WriteMemory(UBrgUIManager.ABrgPawn_BaseNative.mNoDamage, "int", "0");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error disabling No Damage: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            // Check if the game is not running
            else if (Starfire_Status.Text == "STATUS: N/A")
            {
                if (NoDamage_Toggle.Checked)
                {
                    // Force disable the feature if the game is not running
                    NoDamage_Toggle.Checked = false;
                    NoDamage_Label.Text = "- NO DAMAGE: DISABLED";
                }
            }
            // Optional: Handle any other unexpected status
            else
            {
                MessageBox.Show("Unexpected game status. Please check the game state.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void InfiniteHP_Timer_Tick(object sender, EventArgs e)
        {
            int pCHPMAX = M.ReadInt(UBrgUIManager.APawn.HealthMax);
            M.WriteMemory(UBrgUIManager.APawn.Health, "int", pCHPMAX.ToString(CultureInfo.InvariantCulture));
        }

        private void InfiniteHP_Toggle_CheckedChanged(object sender, EventArgs e)
        {
            // Check if the game is currently running
            if (Starfire_Status.Text == "STATUS: GAME FOUND!")
            {
                if (InfiniteHP_Toggle.Checked)
                {
                    // Enable Infinite Health when the toggle is checked
                    InfiniteHP_Label.Text = "- INFINITE HEALTH: ENABLED";
                    InfiniteHP_Timer.Start();
                }
                else
                {
                    // Disable Infinite Health when the toggle is unchecked
                    InfiniteHP_Label.Text = "- INFINITE HEALTH: DISABLED";
                    InfiniteHP_Timer.Stop();
                }
            }
            // Check if the game is not running
            else if (Starfire_Status.Text == "STATUS: N/A")
            {
                if (InfiniteHP_Toggle.Checked)
                {
                    // Force disable the feature if the game is not running
                    InfiniteHP_Label.Text = "- INFINITE HEALTH: DISABLED";
                    InfiniteHP_Toggle.Checked = false;
                    // Note: We don't need to stop the timer here as it wasn't started
                }
            }
            // Optional: Handle any other unexpected status
            else
            {
                MessageBox.Show("Unexpected game status. Please check the game state.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void InfiniteStamina_Timer_Tick(object sender, EventArgs e)
        {
            float pStaminaMax = M.ReadFloat(UBrgUIManager.ABrgPawn_BaseNative.mStaminaMax);
            M.WriteMemory(UBrgUIManager.ABrgPawn_BaseNative.mStamina, "float", pStaminaMax.ToString());
        }

        private void InfiniteStamina_Toggle_CheckedChanged(object sender, EventArgs e)
        {
            // Check if the game is currently running
            if (Starfire_Status.Text == "STATUS: GAME FOUND!")
            {
                if (InfiniteStamina_Toggle.Checked)
                {
                    // Enable Infinite Stamina when the toggle is checked
                    InfiniteStamina_Label.Text = "- INFINITE STAMINA: ENABLED";
                    InfiniteStamina_Timer.Start();
                }
                else
                {
                    // Disable Infinite Stamina when the toggle is unchecked
                    InfiniteStamina_Label.Text = "- INFINITE STAMINA: DISABLED";
                    InfiniteStamina_Timer.Stop();
                }
            }
            // Check if the game is not running
            else if (Starfire_Status.Text == "STATUS: N/A")
            {
                if (InfiniteStamina_Toggle.Checked)
                {
                    // Force disable the feature if the game is not running
                    InfiniteStamina_Label.Text = "- INFINITE STAMINA: DISABLED";
                    InfiniteStamina_Toggle.Checked = false;
                    // Note: We don't need to stop the timer here as it wasn't started
                }
            }
            // Optional: Handle any other unexpected status
            else
            {
                MessageBox.Show("Unexpected game status. Please check the game state.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void InfiniteRage_Timer_Tick(object sender, EventArgs e)
        {
            int pRageMeterMax = M.ReadInt(UBrgUIManager.ABrgCommonPawn_CustomCharaNative.mSkillMoveGaugeStockMax);
            M.WriteMemory(UBrgUIManager.ABrgCommonPawn_CustomCharaNative.mSkillMoveGaugeStockNum, "int", pRageMeterMax.ToString());
        }

        private void InfiniteRage_Toggle_CheckedChanged(object sender, EventArgs e)
        {
            // Check if the game is currently running
            if (Starfire_Status.Text == "STATUS: GAME FOUND!")
            {
                if (InfiniteRage_Toggle.Checked)
                {
                    // Enable Infinite Rage when the toggle is checked
                    InfiniteRage_Label.Text = "- INFINITE RAGE METER: ENABLED";
                    InfiniteRage_Timer.Start();
                }
                else
                {
                    // Disable Infinite Rage when the toggle is unchecked
                    InfiniteRage_Label.Text = "- INFINITE RAGE METER: DISABLED";
                    InfiniteRage_Timer.Stop();
                }
            }
            // Check if the game is not running
            else if (Starfire_Status.Text == "STATUS: N/A")
            {
                if (InfiniteRage_Toggle.Checked)
                {
                    // Force disable the feature if the game is not running
                    InfiniteRage_Label.Text = "- INFINITE RAGE METER: DISABLED";
                    InfiniteRage_Toggle.Checked = false;
                    // Note: We don't need to stop the timer here as it wasn't started
                }
            }
            // Optional: Handle any other unexpected status
            else
            {
                MessageBox.Show("Unexpected game status. Please check the game state.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void InfiniteDurability_Timer_Tick(object sender, EventArgs e)
        {
            try
            {
                // Attempt to write an integer value to the memory address controlling item durability,
                // effectively setting durability decrease to disabled when the timer ticks.
                if (InfiniteDurability_Timer.Enabled)
                {
                    M.WriteMemory(UBrgUIManager.AActor.mDurabilityDownDisable, "int", "64");
                }
                else
                {
                    M.WriteMemory(UBrgUIManager.AActor.mDurabilityDownDisable, "int", "32");
                }
            }
            catch (Exception ex)
            {
                // An exception occurred during the memory write operation.
                // Show a message box to the user detailing the error and provide immediate feedback.
                MessageBox.Show($"Error writing memory for Infinite Durability: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                // Disable the timer to stop further attempts and prevent continuous error prompts.
                InfiniteDurability_Timer.Enabled = false;

                // Update the GUI to reflect that the Infinite Durability feature has been disabled due to an error.
                InfiniteDurability_Toggle.Checked = false;
                InfiniteDurability_Label.Text = "- INFINITE DURABILITY: DISABLED";
            }
        }

        private void InfiniteDurability_Toggle_CheckedChanged(object sender, EventArgs e)
        {
            // Check if the game is currently running
            if (Starfire_Status.Text == "STATUS: GAME FOUND!")
            {
                if (InfiniteDurability_Toggle.Checked)
                {
                    // Enable Infinite Durability when the toggle is checked
                    InfiniteDurability_Label.Text = "- INFINITE DURABILITY: ENABLED";
                    InfiniteDurability_Timer.Start();
                }
                else
                {
                    // Disable Infinite Durability when the toggle is unchecked
                    InfiniteDurability_Label.Text = "- INFINITE DURABILITY: DISABLED";
                    InfiniteDurability_Timer.Stop();
                    try
                    {
                        M.WriteMemory(UBrgUIManager.AActor.mDurabilityDownDisable, "int", "32");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error resetting durability: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            // Check if the game is not running
            else if (Starfire_Status.Text == "STATUS: N/A")
            {
                if (InfiniteDurability_Toggle.Checked)
                {
                    // Force disable the feature if the game is not running
                    InfiniteDurability_Toggle.Checked = false;
                    InfiniteDurability_Label.Text = "- INFINITE DURABILITY: DISABLED";
                    InfiniteDurability_Timer.Stop();
                }
            }
            // Optional: Handle any other unexpected status
            else
            {
                MessageBox.Show("Unexpected game status. Please check the game state.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void SpeedhackWalk_Toggle_CheckedChanged(object sender, EventArgs e)
        {
            // Check if the game is currently running
            if (Starfire_Status.Text == "STATUS: GAME FOUND!")
            {
                if (SpeedhackWalk_Toggle.Checked)
                {
                    // Enable Speedhack (Walk) when the toggle is checked
                    SpeedhackWalk_Label.Text = "- SPEEDHACK (WALK): ENABLED";
                    try
                    {
                        M.WriteMemory(UBrgUIManager.ABrgPawn_BaseNative.mRunSpeedPerSecond, "float", "3000");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error enabling Speedhack (Walk): {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        SpeedhackWalk_Toggle.Checked = false;
                    }
                }
                else
                {
                    // Disable Speedhack (Walk) when the toggle is unchecked
                    SpeedhackWalk_Label.Text = "- SPEEDHACK (WALK): DISABLED";
                    try
                    {
                        M.WriteMemory(UBrgUIManager.ABrgPawn_BaseNative.mRunSpeedPerSecond, "float", "540");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error disabling Speedhack (Walk): {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            // Check if the game is not running
            else if (Starfire_Status.Text == "STATUS: N/A")
            {
                if (SpeedhackWalk_Toggle.Checked)
                {
                    // Force disable the feature if the game is not running
                    SpeedhackWalk_Toggle.Checked = false;
                    SpeedhackWalk_Label.Text = "- SPEEDHACK (WALK): DISABLED";
                }
            }
            // Optional: Handle any other unexpected status
            else
            {
                MessageBox.Show("Unexpected game status. Please check the game state.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void SpeedhackSprint_Toggle_CheckedChanged(object sender, EventArgs e)
        {
            // Check if the game is currently running
            if (Starfire_Status.Text == "STATUS: GAME FOUND!")
            {
                if (SpeedhackSprint_Toggle.Checked)
                {
                    // Enable Speedhack (Sprint) when the toggle is checked
                    SpeedhackSprint_Label.Text = "- SPEEDHACK (SPRINT): ENABLED";
                    try
                    {
                        M.WriteMemory(UBrgUIManager.ABrgPawn_BaseNative.mDashSpeedPerSecond, "float", "3000");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error enabling Speedhack (Sprint): {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        SpeedhackSprint_Toggle.Checked = false;
                    }
                }
                else
                {
                    // Disable Speedhack (Sprint) when the toggle is unchecked
                    SpeedhackSprint_Label.Text = "- SPEEDHACK (SPRINT): DISABLED";
                    try
                    {
                        M.WriteMemory(UBrgUIManager.ABrgPawn_BaseNative.mDashSpeedPerSecond, "float", "850");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error disabling Speedhack (Sprint): {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            // Check if the game is not running
            else if (Starfire_Status.Text == "STATUS: N/A")
            {
                if (SpeedhackSprint_Toggle.Checked)
                {
                    // Force disable the feature if the game is not running
                    SpeedhackSprint_Toggle.Checked = false;
                    SpeedhackSprint_Label.Text = "- SPEEDHACK (SPRINT): DISABLED";
                }
            }
            // Optional: Handle any other unexpected status
            else
            {
                MessageBox.Show("Unexpected game status. Please check the game state.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void SpeedhackCarryWalk_Toggle_CheckedChanged(object sender, EventArgs e)
        {
            // Check if the game is currently running
            if (Starfire_Status.Text == "STATUS: GAME FOUND!")
            {
                if (SpeedhackCarryWalk_Toggle.Checked)
                {
                    // Enable Speedhack (Carry Walk) when the toggle is checked
                    SpeedhackCarryWalk_Label.Text = "- SPEEDHACK (CARRY WALK): ENABLED";
                    try
                    {
                        M.WriteMemory(UBrgUIManager.ABrgPawn_BaseNative.mCarryWalkSpeedPerSecond, "float", "3000");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error enabling Speedhack (Carry Walk): {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        SpeedhackCarryWalk_Toggle.Checked = false;
                    }
                }
                else
                {
                    // Disable Speedhack (Carry Walk) when the toggle is unchecked
                    SpeedhackCarryWalk_Label.Text = "- SPEEDHACK (CARRY WALK): DISABLED";
                    try
                    {
                        M.WriteMemory(UBrgUIManager.ABrgPawn_BaseNative.mCarryWalkSpeedPerSecond, "float", "150");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error disabling Speedhack (Carry Walk): {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            // Check if the game is not running
            else if (Starfire_Status.Text == "STATUS: N/A")
            {
                if (SpeedhackCarryWalk_Toggle.Checked)
                {
                    // Force disable the feature if the game is not running
                    SpeedhackCarryWalk_Toggle.Checked = false;
                    SpeedhackCarryWalk_Label.Text = "- SPEEDHACK (CARRY WALK): DISABLED";
                }
            }
            // Optional: Handle any other unexpected status
            else
            {
                MessageBox.Show("Unexpected game status. Please check the game state.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void SpeedhackCarrySprint_Toggle_CheckedChanged(object sender, EventArgs e)
        {
            // Check if the game is currently running
            if (Starfire_Status.Text == "STATUS: GAME FOUND!")
            {
                try
                {
                    if (SpeedhackCarrySprint_Toggle.Checked)
                    {
                        // Enable Speedhack (Carry Sprint) when the toggle is checked
                        SpeedhackCarrySprint_Label.Text = "- SPEEDHACK (CARRY SPRINT): ENABLED";
                        M.WriteMemory(UBrgUIManager.ABrgPawn_BaseNative.mCarryRunSpeedPerSecond, "float", "3000");
                    }
                    else
                    {
                        // Disable Speedhack (Carry Sprint) when the toggle is unchecked
                        SpeedhackCarrySprint_Label.Text = "- SPEEDHACK (CARRY SPRINT): DISABLED";
                        M.WriteMemory(UBrgUIManager.ABrgPawn_BaseNative.mCarryRunSpeedPerSecond, "float", "450");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error modifying Speedhack (Carry Sprint): {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    // Ensure the toggle reflects the disabled state on error
                    SpeedhackCarrySprint_Toggle.Checked = false;
                    SpeedhackCarrySprint_Label.Text = "- SPEEDHACK (CARRY SPRINT): DISABLED";
                }
            }
            // Check if the game is not running
            else if (Starfire_Status.Text == "STATUS: N/A")
            {
                if (SpeedhackCarrySprint_Toggle.Checked)
                {
                    // Force disable the feature if the game is not running
                    SpeedhackCarrySprint_Toggle.Checked = false;
                    SpeedhackCarrySprint_Label.Text = "- SPEEDHACK (CARRY SPRINT): DISABLED";
                }
            }
            // Optional: Handle any other unexpected status
            else
            {
                MessageBox.Show("Unexpected game status. Please check the game state.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        private void SuperJump_Toggle_CheckedChanged(object sender, EventArgs e)
        {
            // Check if the game is currently running
            if (Starfire_Status.Text == "STATUS: GAME FOUND!")
            {
                if (SuperJump_Toggle.Checked)
                {
                    // Enable Super Jump when the toggle is checked
                    SuperJump_Label.Text = "- SUPER JUMP: ENABLED";
                    try
                    {
                        M.WriteMemory(UBrgUIManager.ABrgPawn_Base.mJumpStartPower, "float", "3000");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error enabling Super Jump: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        SuperJump_Toggle.Checked = false;
                    }
                }
                else
                {
                    // Disable Super Jump when the toggle is unchecked
                    SuperJump_Label.Text = "- SUPER JUMP: DISABLED";
                    try
                    {
                        M.WriteMemory(UBrgUIManager.ABrgPawn_Base.mJumpStartPower, "float", "1100");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error disabling Super Jump: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            // Check if the game is not running
            else if (Starfire_Status.Text == "STATUS: N/A")
            {
                if (SuperJump_Toggle.Checked)
                {
                    // Force disable Super Jump if the game is not running
                    SuperJump_Toggle.Checked = false;
                    SuperJump_Label.Text = "- SUPER JUMP: DISABLED";
                }
            }
            // Optional: Handle any other unexpected status
            else
            {
                MessageBox.Show("Unexpected game status. Please check the game state.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void OneHitKill_Toggle_CheckedChanged(object sender, EventArgs e)
        {
            // Check if the game is currently running
            if (Starfire_Status.Text == "STATUS: GAME FOUND!")
            {
                if (OneHitKill_Toggle.Checked)
                {
                    try
                    {
                        // Uncheck AtkUpScale_Toggle if it is checked
                        if (AtkUpScale_Toggle.Checked)
                        {
                            AtkUpScale_Toggle.Checked = false;
                        }

                        // Write the value for one-hit kill
                        M.WriteMemory(UBrgUIManager.ABrgCommonPawn_CustomChara.mBaseAtkUpScale, "float", "1000000");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error enabling One Hit Kill: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    try
                    {
                        // Write the default value when OneHitKill_Toggle is unchecked
                        M.WriteMemory(UBrgUIManager.ABrgCommonPawn_CustomChara.mBaseAtkUpScale, "float", "1");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error disabling One Hit Kill: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            // Check if the game is not running
            else if (Starfire_Status.Text == "STATUS: N/A")
            {
                if (OneHitKill_Toggle.Checked)
                {
                    // Force disable OneHitKill_Toggle if the game is not running
                    OneHitKill_Toggle.Checked = false;
                }
            }
            // Optional: Handle any other unexpected status
            else
            {
                MessageBox.Show("Unexpected game status. Please check the game state.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void NoRecoil_Timer_Tick(object sender, EventArgs e)
        {
            try
            {
                // Write memory values to enable or disable the No Recoil feature based on the timer's state.
                if (NoRecoil_Timer.Enabled)
                {
                    // Setting all related memory addresses to minimize recoil when the timer is enabled.
                    M.WriteMemory(UBrgUIManager.PlayerCtrlCustomChara.mRecoil, "byte", "1");
                    M.WriteMemory(UBrgUIManager.ABrgCommonPawn_CustomCharaNative.FBrgSkillStatus.RecoilDownRate, "float", "1");
                    M.WriteMemory(UBrgUIManager.ABrgCommonPawn_CustomCharaNative.FBrgSkillStatus.LessDiffusionRate, "float", "1");
                    M.WriteMemory(UBrgUIManager.ABrgCommonPawn_CustomCharaNative.FBrgSkillStatus.LessDiffusionRateRevolver, "float", "1");
                    M.WriteMemory(UBrgUIManager.ABrgCommonPawn_CustomCharaNative.FBrgSkillStatus.LessDiffusionRateShotGun, "float", "1");
                    M.WriteMemory(UBrgUIManager.ABrgCommonPawn_CustomCharaNative.FBrgSkillStatus.RecoilDownRate, "float", "1");
                    M.WriteMemory(UBrgUIManager.PlayerCtrlCustomChara.mRecoilVerticalTarget, "float", "0");
                    M.WriteMemory(UBrgUIManager.PlayerCtrlCustomChara.mRecoilVerticalTotal, "float", "0");
                    M.WriteMemory(UBrgUIManager.PlayerCtrlCustomChara.mRecoilHorizontalTarget, "float", "0");
                    M.WriteMemory(UBrgUIManager.PlayerCtrlCustomChara.mRecoilHorizontalTotal, "float", "0");
                    M.WriteMemory(UBrgUIManager.PlayerCtrlCustomChara.mRecoilNoAimAdjustRate, "float", "0");
                }
                else
                {
                    // Reset all memory addresses related to recoil to their default state when the timer is disabled.
                    M.WriteMemory(UBrgUIManager.PlayerCtrlCustomChara.mRecoil, "byte", "0");
                    M.WriteMemory(UBrgUIManager.ABrgCommonPawn_CustomCharaNative.FBrgSkillStatus.RecoilDownRate, "float", "0");
                    M.WriteMemory(UBrgUIManager.ABrgCommonPawn_CustomCharaNative.FBrgSkillStatus.LessDiffusionRate, "float", "0");
                    M.WriteMemory(UBrgUIManager.ABrgCommonPawn_CustomCharaNative.FBrgSkillStatus.LessDiffusionRateRevolver, "float", "0");
                    M.WriteMemory(UBrgUIManager.ABrgCommonPawn_CustomCharaNative.FBrgSkillStatus.LessDiffusionRateShotGun, "float", "0");
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur during the memory write operation.
                // Display an error message and disable the No Recoil feature.
                MessageBox.Show($"Error adjusting recoil settings: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                // Disable the timer and update the GUI to indicate that the No Recoil feature is disabled.
                NoRecoil_Timer.Enabled = false;
                NoRecoil_Toggle.Checked = false;
                NoRecoil_Label.Text = "- NO RECOIL: DISABLED";
            }
        }

        private void NoRecoil_Toggle_CheckedChanged(object sender, EventArgs e)
        {
            // Check if the game is currently running
            if (Starfire_Status.Text == "STATUS: GAME FOUND!")
            {
                if (NoRecoil_Toggle.Checked)
                {
                    // Enable No Recoil when the toggle is checked
                    NoRecoil_Label.Text = "- NO RECOIL: ENABLED";
                    try
                    {
                        NoRecoil_Timer.Start();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error enabling No Recoil: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        NoRecoil_Toggle.Checked = false;
                    }
                }
                else
                {
                    // Disable No Recoil when the toggle is unchecked
                    NoRecoil_Label.Text = "- NO RECOIL: DISABLED";
                    try
                    {
                        NoRecoil_Timer.Stop();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error disabling No Recoil: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            // Check if the game is not running
            else if (Starfire_Status.Text == "STATUS: N/A")
            {
                if (NoRecoil_Toggle.Checked)
                {
                    // Force disable No Recoil if the game is not running
                    NoRecoil_Toggle.Checked = false;
                    NoRecoil_Label.Text = "- NO RECOIL: DISABLED";
                }
            }
            // Optional: Handle any other unexpected status
            else
            {
                MessageBox.Show("Unexpected game status. Please check the game state.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void InfiniteAmmo_Toggle_CheckedChanged(object sender, EventArgs e)
        {
            // Check if the game is currently running
            if (Starfire_Status.Text == "STATUS: GAME FOUND!")
            {
                if (InfiniteAmmo_Toggle.Checked)
                {
                    // Enable Infinite Ammo when the toggle is checked
                    InfiniteAmmo_Label.Text = "- INFINITE AMMO: ENABLED";
                    try
                    {
                        M.WriteMemory(UBrgUIManager.ABrgPawn_BaseNative.mGunBulletConsumptionDisable, "byte", "08");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error enabling Infinite Ammo: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        InfiniteAmmo_Toggle.Checked = false;
                    }
                }
                else
                {
                    // Disable Infinite Ammo when the toggle is unchecked
                    InfiniteAmmo_Label.Text = "- INFINITE AMMO: DISABLED";
                    try
                    {
                        M.WriteMemory(UBrgUIManager.ABrgPawn_BaseNative.mGunBulletConsumptionDisable, "byte", "00");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error disabling Infinite Ammo: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            // Check if the game is not running
            else if (Starfire_Status.Text == "STATUS: N/A")
            {
                if (InfiniteAmmo_Toggle.Checked)
                {
                    // Force disable Infinite Ammo if the game is not running
                    InfiniteAmmo_Toggle.Checked = false;
                    InfiniteAmmo_Label.Text = "- INFINITE AMMO: DISABLED";
                }
            }
            // Optional: Handle any other unexpected status
            else
            {
                MessageBox.Show("Unexpected game status. Please check the game state.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void TPose_Timer_Tick(object sender, EventArgs e)
        {
            try
            {
                // Attempt to write an integer value to the memory to enable the T-Pose feature.
                M.WriteMemory(UBrgUIManager.ABrgGameInfoNative.ABrgPawn_Base.PlayerBase.UBrgSkeletalMeshComponent.bForceRefpose, "int", "1");
            }
            catch (Exception ex)
            {
                // An exception occurred during the memory write operation.
                // Show a message box to the user detailing the error.
                MessageBox.Show($"Error writing memory for T-Pose: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                // Disable the timer to stop further attempts, preventing repeated errors.
                TPose_Timer.Enabled = false;

                // Update the GUI to reflect that the T-Pose feature has been disabled due to an error.
                TPose_Toggle.Checked = false;
                TPose_Label.Text = "- TPOSE: DISABLED";
            }
        }

        private void TPose_Toggle_CheckedChanged(object sender, EventArgs e)
        {
            // Check if the game is currently running
            if (Starfire_Status.Text == "STATUS: GAME FOUND!")
            {
                if (TPose_Toggle.Checked)
                {
                    // Enable T Pose when the toggle is checked
                    TPose_Label.Text = "- T POSE: ENABLED";
                    try
                    {
                        TPose_Timer.Enabled = true;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error enabling T Pose: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        TPose_Toggle.Checked = false;
                    }
                }
                else
                {
                    // Disable T Pose when the toggle is unchecked
                    TPose_Label.Text = "- T POSE: DISABLED";
                    try
                    {
                        TPose_Timer.Enabled = false;
                        M.WriteMemory(UBrgUIManager.ABrgGameInfoNative.ABrgPawn_Base.PlayerBase.UBrgSkeletalMeshComponent.bForceRefpose, "int", "0");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error disabling T Pose: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            // Check if the game is not running
            else if (Starfire_Status.Text == "STATUS: N/A")
            {
                if (TPose_Toggle.Checked)
                {
                    // Force disable T Pose if the game is not running
                    TPose_Toggle.Checked = false;
                    TPose_Label.Text = "- T POSE: DISABLED";
                }
            }
            // Optional: Handle any other unexpected status
            else
            {
                MessageBox.Show("Unexpected game status. Please check the game state.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void NoFallDamage_Toggle_CheckedChanged(object sender, EventArgs e)
        {
            // Check if the game is currently running
            if (Starfire_Status.Text == "STATUS: GAME FOUND!")
            {
                if (NoFallDamage_Toggle.Checked)
                {
                    // Enable No Fall Damage when the toggle is checked
                    NoFallDamage_Label.Text = "- NO FALL DAMAGE: ENABLED";
                    try
                    {
                        M.WriteMemory(UBrgUIManager.ABrgPawn_BaseNative.mFallDamageStartHeight, "float", "99999999");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error enabling No Fall Damage: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        NoFallDamage_Toggle.Checked = false;
                    }
                }
                else
                {
                    // Disable No Fall Damage when the toggle is unchecked
                    NoFallDamage_Label.Text = "- NO FALL DAMAGE: DISABLED";
                    try
                    {
                        M.WriteMemory(UBrgUIManager.ABrgPawn_BaseNative.mFallDamageStartHeight, "float", "600");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error disabling No Fall Damage: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            // Check if the game is not running
            else if (Starfire_Status.Text == "STATUS: N/A")
            {
                if (NoFallDamage_Toggle.Checked)
                {
                    // Force disable No Fall Damage if the game is not running
                    NoFallDamage_Toggle.Checked = false;
                    NoFallDamage_Label.Text = "- NO FALL DAMAGE: DISABLED";
                }
            }
            // Optional: Handle any other unexpected status
            else
            {
                MessageBox.Show("Unexpected game status. Please check the game state.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void UnFogMiniMap_Timer_Tick(object sender, EventArgs e)
        {
            try
            {
                if (UnFogMiniMap_Timer.Enabled)
                {
                    M.WriteMemory(UBrgUIManager.UBrgUIMiniMapManager.mMiniMapSpraySizeScale, "float", "9999999");
                }
                else
                {
                    M.WriteMemory(UBrgUIManager.UBrgUIMiniMapManager.mMiniMapSpraySizeScale, "float", "90");
                }
            }
            catch (NullReferenceException ex)
            {
                // Handle null reference exception
                MessageBox.Show($"Null reference error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                // Handle any other exceptions
                MessageBox.Show($"Error writing memory: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UnFogMiniMap_Toggle_CheckedChanged(object sender, EventArgs e)
        {
            // Check if the game is currently running
            if (Starfire_Status.Text == "STATUS: GAME FOUND!")
            {
                if (UnFogMiniMap_Toggle.Checked)
                {
                    // Enable UnFog MiniMap when the toggle is checked
                    UnFogMiniMap_Label.Text = "- UN FOG MINI MAP: ENABLED";
                    try
                    {
                        UnFogMiniMap_Timer.Start();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error enabling UnFog MiniMap: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        UnFogMiniMap_Toggle.Checked = false;
                    }
                }
                else
                {
                    // Disable UnFog MiniMap when the toggle is unchecked
                    UnFogMiniMap_Label.Text = "- UN FOG MINI MAP: DISABLED";
                    try
                    {
                        UnFogMiniMap_Timer.Stop();
                        M.WriteMemory(UBrgUIManager.UBrgUIMiniMapManager.mMiniMapSpraySizeScale, "float", "90");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error disabling UnFog MiniMap: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            // Check if the game is not running
            else if (Starfire_Status.Text == "STATUS: N/A")
            {
                if (UnFogMiniMap_Toggle.Checked)
                {
                    // Force disable UnFog MiniMap if the game is not running
                    UnFogMiniMap_Toggle.Checked = false;
                    UnFogMiniMap_Label.Text = "- UN FOG MINI MAP: DISABLED";
                }
            }
            // Optional: Handle any other unexpected status
            else
            {
                MessageBox.Show("Unexpected game status. Please check the game state.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        private void KillcoinVacuum_Timer_Tick(object sender, EventArgs e)
        {
            try
            {
                // Attempt to write a memory value for the Killcoin Vacuum feature.
                M.WriteMemory(UBrgUIManager.ABrgGameInfoNative.mMoneyVacuumFrame, "float", "999999");
            }
            catch (Exception ex)
            {
                // An exception occurred during the memory write operation.
                // Show a message box to the user detailing the error.
                MessageBox.Show($"Error writing memory for Killcoin Vacuum: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                // Disable the timer to stop further attempts which could lead to repeated errors.
                KillcoinVacuum_Timer.Enabled = false;

                // Uncheck the toggle button on the GUI to reflect that the feature has been disabled.
                KillcoinVacuum_Toggle.Checked = false;

                // Update the label text on the GUI to inform the user that the Killcoin Vacuum feature has been disabled due to an error.
                KillcoinVacuum_Label.Text = "- KILLCOIN VACUUM: DISABLED";
            }
        }

        private void KillcoinVacuum_Toggle_CheckedChanged(object sender, EventArgs e)
        {
            // Check if the game is currently running
            if (Starfire_Status.Text == "STATUS: GAME FOUND!")
            {
                if (KillcoinVacuum_Toggle.Checked)
                {
                    // Enable Killcoin Vacuum when the toggle is checked
                    KillcoinVacuum_Label.Text = "- KILLCOIN VACUUM: ENABLED";
                    try
                    {
                        KillcoinVacuum_Timer.Start();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error enabling Killcoin Vacuum: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        KillcoinVacuum_Toggle.Checked = false;
                    }
                }
                else
                {
                    // Disable Killcoin Vacuum when the toggle is unchecked
                    KillcoinVacuum_Label.Text = "- KILLCOIN VACUUM: DISABLED";
                    try
                    {
                        M.WriteMemory(UBrgUIManager.ABrgGameInfoNative.mMoneyVacuumFrame, "float", "0");
                        KillcoinVacuum_Timer.Stop();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error disabling Killcoin Vacuum: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            // Check if the game is not running
            else if (Starfire_Status.Text == "STATUS: N/A")
            {
                if (KillcoinVacuum_Toggle.Checked)
                {
                    // Force disable Killcoin Vacuum if the game is not running
                    KillcoinVacuum_Toggle.Checked = false;
                    KillcoinVacuum_Label.Text = "- KILLCOIN VACUUM: DISABLED";
                }
            }
            // Optional: Handle any other unexpected status
            else
            {
                MessageBox.Show("Unexpected game status. Please check the game state.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void OpenDailyRewardBox_Timer_Tick(object sender, EventArgs e)
        {
            // Stop the timer
            OpenDailyRewardBox_Timer.Stop();
            // Disable the toggle and update the label
            OpenDailyRewardBox_Toggle.Checked = false;
            OpenDailyRewardBox_Label.Text = "- OPEN DAILY REWARD BOX: DISABLED";
        }

        private void OpenDailyRewardBox_Toggle_CheckedChanged(object sender, EventArgs e)
        {
            // Check if the game is currently running
            if (Starfire_Status.Text == "STATUS: GAME FOUND!")
            {
                if (OpenDailyRewardBox_Toggle.Checked)
                {
                    // Enable Open Daily Reward Box when the toggle is checked
                    OpenDailyRewardBox_Label.Text = "- OPEN DAILY REWARD BOX: ENABLED";
                    try
                    {
                        M.WriteMemory(UBrgUIManager.ABrgGameInfoNative.DailyRewardBox.mbOpen, "int", "38");
                        OpenDailyRewardBox_Timer.Start();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error enabling Open Daily Reward Box: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        OpenDailyRewardBox_Toggle.Checked = false;
                    }
                }
                else
                {
                    // Disable Open Daily Reward Box when the toggle is unchecked
                    OpenDailyRewardBox_Label.Text = "- OPEN DAILY REWARD BOX: DISABLED";
                    try
                    {
                        OpenDailyRewardBox_Timer.Stop();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error disabling Open Daily Reward Box: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            // Check if the game is not running
            else if (Starfire_Status.Text == "STATUS: N/A")
            {
                if (OpenDailyRewardBox_Toggle.Checked)
                {
                    // Force disable Open Daily Reward Box if the game is not running
                    OpenDailyRewardBox_Toggle.Checked = false;
                    OpenDailyRewardBox_Label.Text = "- OPEN DAILY REWARD BOX: DISABLED";
                }
            }
            // Optional: Handle any other unexpected status
            else
            {
                MessageBox.Show("Unexpected game status. Please check the game state.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void InstantSpawnJackals_Timer_Tick(object sender, EventArgs e)
        {
            // Stop the timer
            InstantSpawnJackals_Timer.Stop();
            // Disable the toggle and update the label
            InstantSpawnJackals_Toggle.Checked = false;
            InstantSpawnJackals_Label.Text = "- INSTANT SPAWN JACKALS: DISABLED";
        }
        private void DisableInstantSpawnJackals()
        {
            InstantSpawnJackals_Label.Text = "- INSTANT SPAWN JACKALS: DISABLED";
            try
            {
                // Reset the values to 300 when disabling
                M.WriteMemory(UBrgUIManager.ABrgGameInfoNative.UBrgDatabaseParam.mJackalSpawnStartTime, "float", "300");
                M.WriteMemory(UBrgUIManager.ABrgGameInfoNative.UBrgDatabaseParam.mJackalAwakeSpawnStartTime, "float", "300");
                M.WriteMemory(UBrgUIManager.ABrgGameInfoNative.UBrgDatabaseParam.mJackalRebornSpawnStartTime, "float", "300");
                InstantSpawnJackals_Timer.Stop();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error disabling Instant Spawn Jackals: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InstantSpawnJackals_Toggle_CheckedChanged(object sender, EventArgs e)
        {
            // Check if the game is currently running
            if (Starfire_Status.Text == "STATUS: GAME FOUND!")
            {
                if (InstantSpawnJackals_Toggle.Checked)
                {
                    // Enable Instant Spawn Jackal when the toggle is checked
                    InstantSpawnJackals_Label.Text = "- INSTANT SPAWN JACKALS: ENABLED";
                    try
                    {
                        M.WriteMemory(UBrgUIManager.ABrgGameInfoNative.UBrgDatabaseParam.mJackalSpawnStartTime, "float", "1");
                        M.WriteMemory(UBrgUIManager.ABrgGameInfoNative.UBrgDatabaseParam.mJackalAwakeSpawnStartTime, "float", "1");
                        M.WriteMemory(UBrgUIManager.ABrgGameInfoNative.UBrgDatabaseParam.mJackalRebornSpawnStartTime, "float", "1");
                        M.WriteMemory(UBrgUIManager.ABrgGameInfoNative.UBrgAIDirector.mJackalSpawnTime, "float", "1");
                        InstantSpawnJackals_Timer.Start();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error enabling Instant Spawn Jackal: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        InstantSpawnJackals_Toggle.Checked = false;
                    }
                }
                else
                {
                    // Disable Instant Spawn Jackal when the toggle is unchecked
                    InstantSpawnJackals_Label.Text = "- INSTANT SPAWN JACKALS: DISABLED";
                    try
                    {
                        InstantSpawnJackals_Timer.Stop();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error disabling Instant Spawn Jackals: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            // Check if the game is not running
            else if (Starfire_Status.Text == "STATUS: N/A")
            {
                if (InstantSpawnJackals_Toggle.Checked)
                {
                    // Force disable Instant Spawn Jackal if the game is not running
                    InstantSpawnJackals_Toggle.Checked = false;
                    InstantSpawnJackals_Label.Text = "- INSTANT SPAWN JACKALS: DISABLED";
                }
            }
            // Optional: Handle any other unexpected status
            else
            {
                MessageBox.Show("Unexpected game status. Please check the game state.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region Player Editor Cheats
        private void AtkUpScale_Toggle_CheckedChanged(object sender, EventArgs e)
        {
            // Check if the game is currently running
            if (Starfire_Status.Text == "STATUS: GAME FOUND!")
            {
                if (AtkUpScale_Toggle.Checked)
                {
                    try
                    {
                        // Uncheck OneHitKill_Toggle if it is checked
                        if (OneHitKill_Toggle.Checked)
                        {
                            OneHitKill_Toggle.Checked = false;
                        }

                        // Write the value from AtkUpScale_NumericUpDown
                        M.WriteMemory(UBrgUIManager.ABrgCommonPawn_CustomChara.mBaseAtkUpScale, "float", AtkUpScale_NumericUpDown.Value.ToString(CultureInfo.InvariantCulture));
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error enabling ATK UP RATE: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    try
                    {
                        // Write the default value when AtkUpScale_Toggle is unchecked
                        M.WriteMemory(UBrgUIManager.ABrgCommonPawn_CustomChara.mBaseAtkUpScale, "float", "1");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error disabling ATK UP RATE: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            // Check if the game is not running
            else if (Starfire_Status.Text == "STATUS: N/A")
            {
                if (AtkUpScale_Toggle.Checked)
                {
                    // Force disable AtkUpScale_Toggle if the game is not running
                    AtkUpScale_Toggle.Checked = false;
                }
            }
            // Optional: Handle any other unexpected status
            else
            {
                MessageBox.Show("Unexpected game status. Please check the game state.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void PlayerTimescale_Toogle_CheckedChanged(object sender, EventArgs e)
        {
            // Check if the game is currently running
            if (Starfire_Status.Text == "STATUS: GAME FOUND!")
            {
                if (PlayerTimescale_Toogle.Checked)
                {
                    try
                    {
                        // Uncheck PlayerTimescale_Toogle if it is checked
                        if (PlayerTimescale_Toogle.Checked)
                        {
                            PlayerTimescale_Toogle.Checked = false;
                        }

                        // Write the value from PlayerTimescale_NumericUpDown
                        M.WriteMemory(UBrgUIManager.ABrgPawn_BaseNative.mDefaultTimeScale, "float", PlayerTimescale_NumericUpDown.Value.ToString(CultureInfo.InvariantCulture));
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error enabling ATK UP RATE: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    try
                    {
                        // Write the default value when PlayerTimescale_Toogle is unchecked
                        M.WriteMemory(UBrgUIManager.ABrgPawn_BaseNative.mDefaultTimeScale, "float", "1");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error disabling ATK UP RATE: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            // Check if the game is not running
            else if (Starfire_Status.Text == "STATUS: N/A")
            {
                if (PlayerTimescale_Toogle.Checked)
                {
                    // Force disable PlayerTimescale_Toogle if the game is not running
                    PlayerTimescale_Toogle.Checked = false;
                }
            }
            // Optional: Handle any other unexpected status
            else
            {
                MessageBox.Show("Unexpected game status. Please check the game state.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void WorldTimeScale_Toggle_CheckedChanged(object sender, EventArgs e)
        {
            // Check if the game is currently running
            if (Starfire_Status.Text == "STATUS: GAME FOUND!")
            {
                if (WorldTimeScale_Toggle.Checked)
                {
                    try
                    {
                        // Uncheck WorldTimeScale_Toggle if it is checked
                        if (WorldTimeScale_Toggle.Checked)
                        {
                            WorldTimeScale_Toggle.Checked = false;
                        }

                        // Write the value from WorldTimeScale_NumericUpDown
                        M.WriteMemory(UBrgUIManager.ABrgGameInfoNative.UBrgWorldTimeScaleManager.mDefaultTimeScale, "float", WorldTimeScale_NumericUpDown.Value.ToString(CultureInfo.InvariantCulture));
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error enabling ATK UP RATE: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    try
                    {
                        // Write the default value when WorldTimeScale_Toggle is unchecked
                        M.WriteMemory(UBrgUIManager.ABrgGameInfoNative.UBrgWorldTimeScaleManager.mDefaultTimeScale, "float", "1");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error disabling ATK UP RATE: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            // Check if the game is not running
            else if (Starfire_Status.Text == "STATUS: N/A")
            {
                if (WorldTimeScale_Toggle.Checked)
                {
                    // Force disable WorldTimeScale_Toggle if the game is not running
                    WorldTimeScale_Toggle.Checked = false;
                }
            }
            // Optional: Handle any other unexpected status
            else
            {
                MessageBox.Show("Unexpected game status. Please check the game state.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region Teleport Cheats
        private void Elevator_Normal_Button_Click(object sender, EventArgs e)
        {
            try
            {
                // Attempt to read the X, Y, and Z coordinates of the normal elevator from memory
                float elevator_X = M.ReadFloat(UBrgUIManager.ABrgGameInfoNativeBase.ElevatorLocations.ElevatorLocation1_X);
                float elevator_Y = M.ReadFloat(UBrgUIManager.ABrgGameInfoNativeBase.ElevatorLocations.ElevatorLocation1_Y);
                float elevator_Z = M.ReadFloat(UBrgUIManager.ABrgGameInfoNativeBase.ElevatorLocations.ElevatorLocation1_Z);

                // Check if the X coordinate is valid (non-zero or negative)
                if (elevator_X < 0 || elevator_X != 0)
                {
                    // Update the player's location to the elevator's coordinates
                    M.WriteMemory(UBrgUIManager.ABrgGameInfoNative.ABrgPawn_Base.PlayerBase.Location_X, "float", elevator_X.ToString(CultureInfo.InvariantCulture));
                    M.WriteMemory(UBrgUIManager.ABrgGameInfoNative.ABrgPawn_Base.PlayerBase.Location_Y, "float", elevator_Y.ToString(CultureInfo.InvariantCulture));
                    M.WriteMemory(UBrgUIManager.ABrgGameInfoNative.ABrgPawn_Base.PlayerBase.Location_Z, "float", elevator_Z.ToString(CultureInfo.InvariantCulture));
                }
                else
                {
                    // Show a message if the X coordinate is invalid
                    MessageBox.Show("Invalid Elevator X coordinate.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur during the memory read/write operations
                MessageBox.Show($"Error teleporting to the normal elevator: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void Elevator_VIP_Button_Click(object sender, EventArgs e)
        {
            try
            {
                // Attempt to read the X, Y, and Z coordinates of the VIP elevator from memory
                float elevator_X = M.ReadFloat(UBrgUIManager.ABrgGameInfoNativeBase.ElevatorLocations.ElevatorLocation2_X);
                float elevator_Y = M.ReadFloat(UBrgUIManager.ABrgGameInfoNativeBase.ElevatorLocations.ElevatorLocation2_Y);
                float elevator_Z = M.ReadFloat(UBrgUIManager.ABrgGameInfoNativeBase.ElevatorLocations.ElevatorLocation2_Z);

                // Check if the X coordinate is valid (non-zero or negative)
                if (elevator_X < 0 || elevator_X != 0)
                {
                    // Update the player's location to the VIP elevator's coordinates
                    M.WriteMemory(UBrgUIManager.ABrgGameInfoNative.ABrgPawn_Base.PlayerBase.Location_X, "float", elevator_X.ToString(CultureInfo.InvariantCulture));
                    M.WriteMemory(UBrgUIManager.ABrgGameInfoNative.ABrgPawn_Base.PlayerBase.Location_Y, "float", elevator_Y.ToString(CultureInfo.InvariantCulture));
                    M.WriteMemory(UBrgUIManager.ABrgGameInfoNative.ABrgPawn_Base.PlayerBase.Location_Z, "float", elevator_Z.ToString(CultureInfo.InvariantCulture));
                }
                else
                {
                    // Show a message if the X coordinate is invalid
                    MessageBox.Show("Invalid Elevator X coordinate.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur during the memory read/write operations
                MessageBox.Show($"Error teleporting to the VIP elevator: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Escalator_1_Button_Click(object sender, EventArgs e)
        {
            try
            {
                // Attempt to read the X, Y, and Z coordinates of the normal escalator from memory
                float escalator1_X = M.ReadFloat(UBrgUIManager.ABrgGameInfoNativeBase.EscalatorLocations.EscalatorLocation1_X);
                float escalator1_Y = M.ReadFloat(UBrgUIManager.ABrgGameInfoNativeBase.EscalatorLocations.EscalatorLocation1_Y);
                float escalator1_Z = M.ReadFloat(UBrgUIManager.ABrgGameInfoNativeBase.EscalatorLocations.EscalatorLocation1_Z);

                // Check if the X coordinate is valid (non-zero or negative)
                if (escalator1_X < 0 || escalator1_X != 0)
                {
                    // Update the player's location to the escalator's coordinates
                    M.WriteMemory(UBrgUIManager.ABrgGameInfoNative.ABrgPawn_Base.PlayerBase.Location_X, "float", escalator1_X.ToString(CultureInfo.InvariantCulture));
                    M.WriteMemory(UBrgUIManager.ABrgGameInfoNative.ABrgPawn_Base.PlayerBase.Location_Y, "float", escalator1_Y.ToString(CultureInfo.InvariantCulture));
                    M.WriteMemory(UBrgUIManager.ABrgGameInfoNative.ABrgPawn_Base.PlayerBase.Location_Z, "float", escalator1_Z.ToString(CultureInfo.InvariantCulture));
                }
                else
                {
                    // Show a message if the X coordinate is invalid
                    MessageBox.Show("Invalid Escalator 1 X coordinate.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur during the memory read/write operations
                MessageBox.Show($"Error teleporting to Escalator 1: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Escalator_2_Button_Click(object sender, EventArgs e)
        {
            try
            {
                // Attempt to read the X, Y, and Z coordinates of the normal escalator from memory
                float escalator2_Y = M.ReadFloat(UBrgUIManager.ABrgGameInfoNativeBase.EscalatorLocations.EscalatorLocation2_Y);
                float escalator2_Z = M.ReadFloat(UBrgUIManager.ABrgGameInfoNativeBase.EscalatorLocations.EscalatorLocation2_Z);
                float escalator2_X = M.ReadFloat(UBrgUIManager.ABrgGameInfoNativeBase.EscalatorLocations.EscalatorLocation2_X);

                // Check if the X coordinate is valid (non-zero or negative)
                if (escalator2_X < 0 || escalator2_X != 0)
                {
                    // Update the player's location to the escalator's coordinates
                    M.WriteMemory(UBrgUIManager.ABrgGameInfoNative.ABrgPawn_Base.PlayerBase.Location_X, "float", escalator2_X.ToString(CultureInfo.InvariantCulture));
                    M.WriteMemory(UBrgUIManager.ABrgGameInfoNative.ABrgPawn_Base.PlayerBase.Location_Y, "float", escalator2_Y.ToString(CultureInfo.InvariantCulture));
                    M.WriteMemory(UBrgUIManager.ABrgGameInfoNative.ABrgPawn_Base.PlayerBase.Location_Z, "float", escalator2_Z.ToString(CultureInfo.InvariantCulture));
                }
                else
                {
                    // Show a message if the X coordinate is invalid
                    MessageBox.Show("Invalid Escalator 2 XYZ coordinate.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur during the memory read/write operations
                MessageBox.Show($"Error teleporting to Escalator 2: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Escalator_3_Button_Click(object sender, EventArgs e)
        {
            try
            {
                // Attempt to read the X, Y, and Z coordinates of the normal escalator from memory
                float escalator3_X = M.ReadFloat(UBrgUIManager.ABrgGameInfoNativeBase.EscalatorLocations.EscalatorLocation3_X);
                float escalator3_Y = M.ReadFloat(UBrgUIManager.ABrgGameInfoNativeBase.EscalatorLocations.EscalatorLocation3_Y);
                float escalator3_Z = M.ReadFloat(UBrgUIManager.ABrgGameInfoNativeBase.EscalatorLocations.EscalatorLocation3_Z);

                // Check if the X coordinate is valid (non-zero or negative)
                if (escalator3_X < 0 || escalator3_X != 0)
                {
                    // Update the player's location to the escalator's coordinates
                    M.WriteMemory(UBrgUIManager.ABrgGameInfoNative.ABrgPawn_Base.PlayerBase.Location_X, "float", escalator3_X.ToString(CultureInfo.InvariantCulture));
                    M.WriteMemory(UBrgUIManager.ABrgGameInfoNative.ABrgPawn_Base.PlayerBase.Location_Y, "float", escalator3_Y.ToString(CultureInfo.InvariantCulture));
                    M.WriteMemory(UBrgUIManager.ABrgGameInfoNative.ABrgPawn_Base.PlayerBase.Location_Z, "float", escalator3_Z.ToString(CultureInfo.InvariantCulture));
                }
                else
                {
                    // Show a message if the X coordinate is invalid
                    MessageBox.Show("Invalid Escalator 3 XYZ coordinate.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur during the memory read/write operations
                MessageBox.Show($"Error teleporting to Escalator 3: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Escalator_4_Button_Click(object sender, EventArgs e)
        {
            try
            {
                // Attempt to read the X, Y, and Z coordinates of the normal escalator from memory
                float escalator4_X = M.ReadFloat(UBrgUIManager.ABrgGameInfoNativeBase.EscalatorLocations.EscalatorLocation4_X);
                float escalator4_Y = M.ReadFloat(UBrgUIManager.ABrgGameInfoNativeBase.EscalatorLocations.EscalatorLocation4_Y);
                float escalator4_Z = M.ReadFloat(UBrgUIManager.ABrgGameInfoNativeBase.EscalatorLocations.EscalatorLocation4_Z);

                // Check if the X coordinate is valid (non-zero or negative)
                if (escalator4_X < 0 || escalator4_X != 0)
                {
                    // Update the player's location to the escalator's coordinates
                    M.WriteMemory(UBrgUIManager.ABrgGameInfoNative.ABrgPawn_Base.PlayerBase.Location_X, "float", escalator4_X.ToString(CultureInfo.InvariantCulture));
                    M.WriteMemory(UBrgUIManager.ABrgGameInfoNative.ABrgPawn_Base.PlayerBase.Location_Y, "float", escalator4_Y.ToString(CultureInfo.InvariantCulture));
                    M.WriteMemory(UBrgUIManager.ABrgGameInfoNative.ABrgPawn_Base.PlayerBase.Location_Z, "float", escalator4_Z.ToString(CultureInfo.InvariantCulture));
                }
                else
                {
                    // Show a message if the X coordinate is invalid
                    MessageBox.Show("Invalid Escalator 4 XYZ coordinate.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur during the memory read/write operations
                MessageBox.Show($"Error teleporting to Escalator 4: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BackToBaseFloor_Button_Click(object sender, EventArgs e)
        {
            // Write value to utilize in game error to send player safely back to base floor! (error does not issue any bans aka PLAYER EMERGENCY SEND TO BASE FLOOR!)
            M.WriteMemory(UBrgUIManager.ABrgGameInfo.mbPlayerEmergency, "int", "32");
        }

        private void StampRallyTable_Button_Click(object sender, EventArgs e)
        {
            try
            {
                // Attempt to read the X, Y, and Z coordinates of the stamp rally table's from memory
                float StampR_X = M.ReadFloat(UBrgUIManager.ABrgGameInfoNativeBase.StampTableLocation.StampTable_X);
                float StampR_Y = M.ReadFloat(UBrgUIManager.ABrgGameInfoNativeBase.StampTableLocation.StampTable_Y);
                float StampR_Z = M.ReadFloat(UBrgUIManager.ABrgGameInfoNativeBase.StampTableLocation.StampTable_Z);

                // Check if the X coordinate is valid (non-zero or negative)
                if (StampR_X < 0 || StampR_X != 0)
                {
                    // Update the player's location to the stamp rally table's coordinates
                    M.WriteMemory(UBrgUIManager.ABrgGameInfoNative.ABrgPawn_Base.PlayerBase.Location_X, "float", StampR_X.ToString(CultureInfo.InvariantCulture));
                    M.WriteMemory(UBrgUIManager.ABrgGameInfoNative.ABrgPawn_Base.PlayerBase.Location_Y, "float", StampR_Y.ToString(CultureInfo.InvariantCulture));
                    M.WriteMemory(UBrgUIManager.ABrgGameInfoNative.ABrgPawn_Base.PlayerBase.Location_Z, "float", StampR_Z.ToString(CultureInfo.InvariantCulture));
                }
                else
                {
                    // Show a message if the X coordinate is invalid
                    MessageBox.Show("Invalid Stamp Rally XYZ coordinate.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur during the memory read/write operations
                MessageBox.Show($"Error teleporting to Stamp Rally Table: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Resource1_Teleport_Click(object sender, EventArgs e)
        {
            try
            {
                // Attempt to read the X, Y, and Z coordinates of resource 1 from memory
                float resource1_X = M.ReadFloat(UBrgUIManager.ABrgGameInfoNativeBase.MaterialLocations.MaterialLocation1_X);
                float resource1_Y = M.ReadFloat(UBrgUIManager.ABrgGameInfoNativeBase.MaterialLocations.MaterialLocation1_Y);
                float resource1_Z = M.ReadFloat(UBrgUIManager.ABrgGameInfoNativeBase.MaterialLocations.MaterialLocation1_Z);

                // Check if the X coordinate is valid (non-zero or negative)
                if (resource1_X < 0 || resource1_X != 0)
                {
                    // Update the player's location to resource 1 coordinates
                    M.WriteMemory(UBrgUIManager.ABrgGameInfoNative.ABrgPawn_Base.PlayerBase.Location_X, "float", resource1_X.ToString(CultureInfo.InvariantCulture));
                    M.WriteMemory(UBrgUIManager.ABrgGameInfoNative.ABrgPawn_Base.PlayerBase.Location_Y, "float", resource1_Y.ToString(CultureInfo.InvariantCulture));
                    M.WriteMemory(UBrgUIManager.ABrgGameInfoNative.ABrgPawn_Base.PlayerBase.Location_Z, "float", resource1_Z.ToString(CultureInfo.InvariantCulture));
                }
                else
                {
                    // Show a message if the X coordinate is invalid
                    MessageBox.Show("Invalid Resource 1 X coordinate.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur during the memory read/write operations
                MessageBox.Show($"Error teleporting to Resource 1: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Resource2_Teleport_Click(object sender, EventArgs e)
        {
            try
            {
                // Attempt to read the X, Y, and Z coordinates of the resource 2 from memory
                float resource2_X = M.ReadFloat(UBrgUIManager.ABrgGameInfoNativeBase.MaterialLocations.MaterialLocation2_X);
                float resource2_Y = M.ReadFloat(UBrgUIManager.ABrgGameInfoNativeBase.MaterialLocations.MaterialLocation2_Y);
                float resource2_Z = M.ReadFloat(UBrgUIManager.ABrgGameInfoNativeBase.MaterialLocations.MaterialLocation2_Z);

                // Check if the X coordinate is valid (non-zero or negative)
                if (resource2_X < 0 || resource2_X != 0)
                {
                    // Update the player's location to resource 2 coordinates
                    M.WriteMemory(UBrgUIManager.ABrgGameInfoNative.ABrgPawn_Base.PlayerBase.Location_X, "float", resource2_X.ToString(CultureInfo.InvariantCulture));
                    M.WriteMemory(UBrgUIManager.ABrgGameInfoNative.ABrgPawn_Base.PlayerBase.Location_Y, "float", resource2_Y.ToString(CultureInfo.InvariantCulture));
                    M.WriteMemory(UBrgUIManager.ABrgGameInfoNative.ABrgPawn_Base.PlayerBase.Location_Z, "float", resource2_Z.ToString(CultureInfo.InvariantCulture));
                }
                else
                {
                    // Show a message if the X coordinate is invalid
                    MessageBox.Show("Invalid Resource 2 X coordinate.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur during the memory read/write operations
                MessageBox.Show($"Error teleporting to Resource 2: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void Resource3_Teleport_Click(object sender, EventArgs e)
        {
            try
            {
                // Attempt to read the X, Y, and Z coordinates of resource 3 from memory
                float resource3_X = M.ReadFloat(UBrgUIManager.ABrgGameInfoNativeBase.MaterialLocations.MaterialLocation3_X);
                float resource3_Y = M.ReadFloat(UBrgUIManager.ABrgGameInfoNativeBase.MaterialLocations.MaterialLocation3_Y);
                float resource3_Z = M.ReadFloat(UBrgUIManager.ABrgGameInfoNativeBase.MaterialLocations.MaterialLocation3_Z);

                // Check if the X coordinate is valid (non-zero or negative)
                if (resource3_X < 0 || resource3_X != 0)
                {
                    // Update the player's location to resource 3 coordinates
                    M.WriteMemory(UBrgUIManager.ABrgGameInfoNative.ABrgPawn_Base.PlayerBase.Location_X, "float", resource3_X.ToString(CultureInfo.InvariantCulture));
                    M.WriteMemory(UBrgUIManager.ABrgGameInfoNative.ABrgPawn_Base.PlayerBase.Location_Y, "float", resource3_Y.ToString(CultureInfo.InvariantCulture));
                    M.WriteMemory(UBrgUIManager.ABrgGameInfoNative.ABrgPawn_Base.PlayerBase.Location_Z, "float", resource3_Z.ToString(CultureInfo.InvariantCulture));
                }
                else
                {
                    // Show a message if the X coordinate is invalid
                    MessageBox.Show("Invalid Resource 3 X coordinate.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur during the memory read/write operations
                MessageBox.Show($"Error teleporting to Resource 3: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Resource4_Teleport_Click(object sender, EventArgs e)
        {
            try
            {
                // Attempt to read the X, Y, and Z coordinates of resource 4 from memory
                float resource4_X = M.ReadFloat(UBrgUIManager.ABrgGameInfoNativeBase.MaterialLocations.MaterialLocation4_X);
                float resource4_Y = M.ReadFloat(UBrgUIManager.ABrgGameInfoNativeBase.MaterialLocations.MaterialLocation4_Y);
                float resource4_Z = M.ReadFloat(UBrgUIManager.ABrgGameInfoNativeBase.MaterialLocations.MaterialLocation4_Z);

                // Check if the X coordinate is valid (non-zero or negative)
                if (resource4_X < 0 || resource4_X != 0)
                {
                    // Update the player's location to resource 4 coordinates
                    M.WriteMemory(UBrgUIManager.ABrgGameInfoNative.ABrgPawn_Base.PlayerBase.Location_X, "float", resource4_X.ToString(CultureInfo.InvariantCulture));
                    M.WriteMemory(UBrgUIManager.ABrgGameInfoNative.ABrgPawn_Base.PlayerBase.Location_Y, "float", resource4_Y.ToString(CultureInfo.InvariantCulture));
                    M.WriteMemory(UBrgUIManager.ABrgGameInfoNative.ABrgPawn_Base.PlayerBase.Location_Z, "float", resource4_Z.ToString(CultureInfo.InvariantCulture));
                }
                else
                {
                    // Show a message if the X coordinate is invalid
                    MessageBox.Show("Invalid Resource 4 X coordinate.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur during the memory read/write operations
                MessageBox.Show($"Error teleporting to Resource 4: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Resource5_Teleport_Click(object sender, EventArgs e)
        {
            try
            {
                // Attempt to read the X, Y, and Z coordinates of resource 5 from memory
                float resource5_X = M.ReadFloat(UBrgUIManager.ABrgGameInfoNativeBase.MaterialLocations.MaterialLocation5_X);
                float resource5_Y = M.ReadFloat(UBrgUIManager.ABrgGameInfoNativeBase.MaterialLocations.MaterialLocation5_Y);
                float resource5_Z = M.ReadFloat(UBrgUIManager.ABrgGameInfoNativeBase.MaterialLocations.MaterialLocation5_Z);

                // Check if the X coordinate is valid (non-zero or negative)
                if (resource5_X < 0 || resource5_X != 0)
                {
                    // Update the player's location to resource 5 coordinates
                    M.WriteMemory(UBrgUIManager.ABrgGameInfoNative.ABrgPawn_Base.PlayerBase.Location_X, "float", resource5_X.ToString(CultureInfo.InvariantCulture));
                    M.WriteMemory(UBrgUIManager.ABrgGameInfoNative.ABrgPawn_Base.PlayerBase.Location_Y, "float", resource5_Y.ToString(CultureInfo.InvariantCulture));
                    M.WriteMemory(UBrgUIManager.ABrgGameInfoNative.ABrgPawn_Base.PlayerBase.Location_Z, "float", resource5_Z.ToString(CultureInfo.InvariantCulture));
                }
                else
                {
                    // Show a message if the X coordinate is invalid
                    MessageBox.Show("Invalid Resource 5 X coordinate.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur during the memory read/write operations
                MessageBox.Show($"Error teleporting to Resource 5: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void Resource6_Teleport_Click(object sender, EventArgs e)
        {
            try
            {
                // Attempt to read the X, Y, and Z coordinates of resource 6 from memory
                float resource6_X = M.ReadFloat(UBrgUIManager.ABrgGameInfoNativeBase.MaterialLocations.MaterialLocation6_X);
                float resource6_Y = M.ReadFloat(UBrgUIManager.ABrgGameInfoNativeBase.MaterialLocations.MaterialLocation6_Y);
                float resource6_Z = M.ReadFloat(UBrgUIManager.ABrgGameInfoNativeBase.MaterialLocations.MaterialLocation6_Z);

                // Check if the X coordinate is valid (non-zero or negative)
                if (resource6_X < 0 || resource6_X != 0)
                {
                    // Update the player's location to resource 6 coordinates
                    M.WriteMemory(UBrgUIManager.ABrgGameInfoNative.ABrgPawn_Base.PlayerBase.Location_X, "float", resource6_X.ToString(CultureInfo.InvariantCulture));
                    M.WriteMemory(UBrgUIManager.ABrgGameInfoNative.ABrgPawn_Base.PlayerBase.Location_Y, "float", resource6_Y.ToString(CultureInfo.InvariantCulture));
                    M.WriteMemory(UBrgUIManager.ABrgGameInfoNative.ABrgPawn_Base.PlayerBase.Location_Z, "float", resource6_Z.ToString(CultureInfo.InvariantCulture));
                }
                else
                {
                    // Show a message if the X coordinate is invalid
                    MessageBox.Show("Invalid Resource 6 X coordinate.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur during the memory read/write operations
                MessageBox.Show($"Error teleporting to Resource 6: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Resource7_Teleport_Click(object sender, EventArgs e)
        {
            try
            {
                // Attempt to read the X, Y, and Z coordinates of resource 7 from memory
                float resource7_X = M.ReadFloat(UBrgUIManager.ABrgGameInfoNativeBase.MaterialLocations.MaterialLocation7_X);
                float resource7_Y = M.ReadFloat(UBrgUIManager.ABrgGameInfoNativeBase.MaterialLocations.MaterialLocation7_Y);
                float resource7_Z = M.ReadFloat(UBrgUIManager.ABrgGameInfoNativeBase.MaterialLocations.MaterialLocation7_Z);

                // Check if the X coordinate is valid (non-zero or negative)
                if (resource7_X < 0 || resource7_X != 0)
                {
                    // Update the player's location to resource 7 coordinates
                    M.WriteMemory(UBrgUIManager.ABrgGameInfoNative.ABrgPawn_Base.PlayerBase.Location_X, "float", resource7_X.ToString(CultureInfo.InvariantCulture));
                    M.WriteMemory(UBrgUIManager.ABrgGameInfoNative.ABrgPawn_Base.PlayerBase.Location_Y, "float", resource7_Y.ToString(CultureInfo.InvariantCulture));
                    M.WriteMemory(UBrgUIManager.ABrgGameInfoNative.ABrgPawn_Base.PlayerBase.Location_Z, "float", resource7_Z.ToString(CultureInfo.InvariantCulture));
                }
                else
                {
                    // Show a message if the X coordinate is invalid
                    MessageBox.Show("Invalid Resource 7 X coordinate.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur during the memory read/write operations
                MessageBox.Show($"Error teleporting to Resource 7: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Resource8_Teleport_Click(object sender, EventArgs e)
        {
            try
            {
                // Attempt to read the X, Y, and Z coordinates of resource 8 from memory
                float resource8_X = M.ReadFloat(UBrgUIManager.ABrgGameInfoNativeBase.MaterialLocations.MaterialLocation8_X);
                float resource8_Y = M.ReadFloat(UBrgUIManager.ABrgGameInfoNativeBase.MaterialLocations.MaterialLocation8_Y);
                float resource8_Z = M.ReadFloat(UBrgUIManager.ABrgGameInfoNativeBase.MaterialLocations.MaterialLocation8_Z);

                // Check if the X coordinate is valid (non-zero or negative)
                if (resource8_X < 0 || resource8_X != 0)
                {
                    // Update the player's location to resource 8 coordinates
                    M.WriteMemory(UBrgUIManager.ABrgGameInfoNative.ABrgPawn_Base.PlayerBase.Location_X, "float", resource8_X.ToString(CultureInfo.InvariantCulture));
                    M.WriteMemory(UBrgUIManager.ABrgGameInfoNative.ABrgPawn_Base.PlayerBase.Location_Y, "float", resource8_Y.ToString(CultureInfo.InvariantCulture));
                    M.WriteMemory(UBrgUIManager.ABrgGameInfoNative.ABrgPawn_Base.PlayerBase.Location_Z, "float", resource8_Z.ToString(CultureInfo.InvariantCulture));
                }
                else
                {
                    // Show a message if the X coordinate is invalid
                    MessageBox.Show("Invalid Resource 8 X coordinate.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur during the memory read/write operations
                MessageBox.Show($"Error teleporting to Resource 8: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Resource9_Teleport_Click(object sender, EventArgs e)
        {
            try
            {
                // Attempt to read the X, Y, and Z coordinates of resource 9 from memory
                float resource9_X = M.ReadFloat(UBrgUIManager.ABrgGameInfoNativeBase.MaterialLocations.MaterialLocation9_X);
                float resource9_Y = M.ReadFloat(UBrgUIManager.ABrgGameInfoNativeBase.MaterialLocations.MaterialLocation9_Y);
                float resource9_Z = M.ReadFloat(UBrgUIManager.ABrgGameInfoNativeBase.MaterialLocations.MaterialLocation9_Z);

                // Check if the X coordinate is valid (non-zero or negative)
                if (resource9_X < 0 || resource9_X != 0)
                {
                    // Update the player's location to resource 9 coordinates
                    M.WriteMemory(UBrgUIManager.ABrgGameInfoNative.ABrgPawn_Base.PlayerBase.Location_X, "float", resource9_X.ToString(CultureInfo.InvariantCulture));
                    M.WriteMemory(UBrgUIManager.ABrgGameInfoNative.ABrgPawn_Base.PlayerBase.Location_Y, "float", resource9_Y.ToString(CultureInfo.InvariantCulture));
                    M.WriteMemory(UBrgUIManager.ABrgGameInfoNative.ABrgPawn_Base.PlayerBase.Location_Z, "float", resource9_Z.ToString(CultureInfo.InvariantCulture));
                }
                else
                {
                    // Show a message if the X coordinate is invalid
                    MessageBox.Show("Invalid Resource 9 X coordinate.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur during the memory read/write operations
                MessageBox.Show($"Error teleporting to Resource 9: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Resource10_Teleport_Click(object sender, EventArgs e)
        {
            try
            {
                // Attempt to read the X, Y, and Z coordinates of resource 10 from memory
                float resource10_X = M.ReadFloat(UBrgUIManager.ABrgGameInfoNativeBase.MaterialLocations.MaterialLocation10_X);
                float resource10_Y = M.ReadFloat(UBrgUIManager.ABrgGameInfoNativeBase.MaterialLocations.MaterialLocation10_Y);
                float resource10_Z = M.ReadFloat(UBrgUIManager.ABrgGameInfoNativeBase.MaterialLocations.MaterialLocation10_Z);

                // Check if the X coordinate is valid (non-zero or negative)
                if (resource10_X < 0 || resource10_X != 0)
                {
                    // Update the player's location to resource 10 coordinates
                    M.WriteMemory(UBrgUIManager.ABrgGameInfoNative.ABrgPawn_Base.PlayerBase.Location_X, "float", resource10_X.ToString(CultureInfo.InvariantCulture));
                    M.WriteMemory(UBrgUIManager.ABrgGameInfoNative.ABrgPawn_Base.PlayerBase.Location_Y, "float", resource10_Y.ToString(CultureInfo.InvariantCulture));
                    M.WriteMemory(UBrgUIManager.ABrgGameInfoNative.ABrgPawn_Base.PlayerBase.Location_Z, "float", resource10_Z.ToString(CultureInfo.InvariantCulture));
                }
                else
                {
                    // Show a message if the X coordinate is invalid
                    MessageBox.Show("Invalid Resource 10 X coordinate.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur during the memory read/write operations
                MessageBox.Show($"Error teleporting to Resource 10: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        private void Starfire_Header_Panel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
