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
using Memory;
using static KC__LID_EXT.BackEnd.Dump.SDK;
using System.Globalization;

namespace STARFIRE.FrontEnd
{
    public partial class Starfire : Form
    {
        Mem M = new Mem();
        bool IsProcOpen;
        int ProcessID;

        public Starfire()
        {
            InitializeComponent();
            Starfire_BGWorker.RunWorkerAsync();
            // Initialize the form, then set it's state to Normal so handheld devices work.
            this.WindowState = FormWindowState.Normal;
            this.TopMost = true;
        }
        #region EXIT BUTTON
        private void Starfire_Exit_Click(object sender, EventArgs e)
        {
            if (Starfire_Status.Text == "STATUS: GAME FOUND!")
            {
                Invincible_Toggle.Checked = false;
                NoDamage_Toggle.Checked = false;
                InfiniteHP_Toggle.Checked = false;
                InfiniteStamina_Toggle.Checked = false;
                InfiniteRage_Toggle.Checked = false;
                InfiniteDurability_Toggle.Checked = false;
                SpeedhackWalk_Toggle.Checked = false;
                SpeedhackSprint_Toggle.Checked = false;
                SuperJump_Toggle.Checked = false;
                OneHitKill_Toggle.Checked = false;
                NoRecoil_Toggle.Checked = false;
                InfiniteAmmo_Toggle.Checked = false;
                TPose_Toggle.Checked = false;
                NoFallDamage_Toggle.Checked = false;
                UnFogMiniMap_Toggle.Checked = false;
                KillcoinVacuum_Toggle.Checked = false;
                OpenDailyRewardBox_Toggle.Checked = false;
                // Terminate the execution of the current application and exit with a success code (0).
                Environment.Exit(0);
            }
            if (Starfire_Status.Text == "STATUS: N/A")
            {
                // Terminate the execution of the current application and exit with a success code (0).
                Environment.Exit(0);
            }
        }
        #endregion

        #region TrainerUI
        private void Starfire_Load(object sender, EventArgs e)
        {
            // Set the form window state to normal again to insure it is the correct size while on PC or any other device.
            this.WindowState = FormWindowState.Normal;
            Starfire_Status.Text = "STATUS: N/A";
        }

        private void Starfire_TabControl_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
        #endregion
        #region BackgroundWorker
        private void Starfire_BGWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            ProcessID = M.GetProcIdFromName("BrgGame-Steam.exe");
            if (ProcessID != 0)
            {
                IsProcOpen = M.OpenProcess(ProcessID);
                Thread.Sleep(100);
                Starfire_BGWorker.ReportProgress(0);
            }
        }

        private void Starfire_BGWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (IsProcOpen)
            {
                Starfire_Status.Text = "STATUS: GAME FOUND!";
                float pAtkUpScale = M.ReadFloat(UBrgUIManager.ABrgCommonPawn_CustomChara.mBaseAtkUpScale);
                AtkUpScale_Value.Text = "#" + pAtkUpScale.ToString(CultureInfo.InvariantCulture);
            }
            else if (!IsProcOpen)
            {
                Starfire_Status.Text = "STATUS: N/A";
            }
        }

        private void Starfire_BGWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Starfire_BGWorker.RunWorkerAsync();
        }
        #endregion

        #region CheatsTab
        private void Invincible_Toggle_CheckedChanged(object sender, EventArgs e)
        {
            if (Starfire_Status.Text == "STATUS: GAME FOUND!")
            {
                if (Invincible_Toggle.Checked)
                {
                    Invincible_Label.Text = "- INVINCIBLE: ENABLED";

                    M.WriteMemory(UBrgUIManager.ABrgPawn_BaseNative.mInvincible, "int", "1");
                }
                else
                {
                    Invincible_Label.Text = "- INVINCIBLE: DISABLED";
                    M.WriteMemory(UBrgUIManager.ABrgPawn_BaseNative.mInvincible, "int", "0");
                }
            }
            if (Starfire_Status.Text == "STATUS: N/A")
            {
                if (Invincible_Toggle.Checked)
                {
                    Invincible_Toggle.Checked = false;
                }
            }
        }

        private void NoDamage_Toggle_CheckedChanged(object sender, EventArgs e)
        {
            if (Starfire_Status.Text == "STATUS: GAME FOUND!")
            {
                if (NoDamage_Toggle.Checked)
                {
                    NoDamage_Label.Text = "- NO DAMAGE: ENABLED";

                    M.WriteMemory(UBrgUIManager.ABrgPawn_BaseNative.mNoDamage, "int", "1");
                }
                else
                {
                    NoDamage_Label.Text = "- NO DAMAGE: DISABLED";
                    M.WriteMemory(UBrgUIManager.ABrgPawn_BaseNative.mNoDamage, "int", "0");
                }
            }
            if (Starfire_Status.Text == "STATUS: N/A")
            {
                if (NoDamage_Toggle.Checked)
                {
                    NoDamage_Toggle.Checked = false;
                }
            }
        }

        private void InfiniteHP_Timer_Tick(object sender, EventArgs e)
        {
            int pCHPMAX = M.ReadInt(UBrgUIManager.APawn.HealthMax);
            M.WriteMemory(UBrgUIManager.APawn.Health, "int", pCHPMAX.ToString(CultureInfo.InvariantCulture));
        }

        private void InfiniteHP_Toggle_CheckedChanged(object sender, EventArgs e)
        {
            if (Starfire_Status.Text == "STATUS: GAME FOUND!")
            {
                if (InfiniteHP_Toggle.Checked)
                {
                    InfiniteHP_Label.Text = "- INFINITE HEALTH: ENABLED";
                    InfiniteHP_Timer.Start();
                }
                else
                {
                    InfiniteHP_Label.Text = "- INFINITE HEALTH: DISABLED";
                    InfiniteHP_Timer.Stop();
                }
            }
            if (Starfire_Status.Text == "STATUS: N/A")
            {
                if (InfiniteHP_Toggle.Checked)
                {
                    InfiniteHP_Toggle.Checked = false;
                }
            }
        }

        private void InfiniteStamina_Timer_Tick(object sender, EventArgs e)
        {
            float pStaminaMax = M.ReadFloat(UBrgUIManager.ABrgPawn_BaseNative.mStaminaMax);
            M.WriteMemory(UBrgUIManager.ABrgPawn_BaseNative.mStamina, "float", pStaminaMax.ToString());
        }

        private void InfiniteStamina_Toggle_CheckedChanged(object sender, EventArgs e)
        {
            if (InfiniteStamina_Toggle.Checked)
            {
                InfiniteStamina_Label.Text = "- INFINITE STAMINA: ENABLED";
                InfiniteStamina_Timer.Start();
            }
            else
            {
                InfiniteStamina_Label.Text = "- INFINITE STAMINA: DISABLED";
                InfiniteStamina_Timer.Stop();
            }
        }

        private void InfiniteRage_Timer_Tick(object sender, EventArgs e)
        {
            int pRageMeterMax = M.ReadInt(UBrgUIManager.ABrgCommonPawn_CustomCharaNative.mSkillMoveGaugeStockMax);
            M.WriteMemory(UBrgUIManager.ABrgCommonPawn_CustomCharaNative.mSkillMoveGaugeStockNum, "int", pRageMeterMax.ToString());
        }

        private void InfiniteRage_Toggle_CheckedChanged(object sender, EventArgs e)
        {
            if (InfiniteRage_Toggle.Checked)
            {
                InfiniteRage_Label.Text = "- INFINITE RAGE METER: ENABLED";
                InfiniteRage_Timer.Start();
            }
            else
            {
                InfiniteRage_Label.Text = "- INFINITE RAGE METER: DISABLED";
                InfiniteRage_Timer.Stop();
            }
        }

        private void InfiniteDurability_Timer_Tick(object sender, EventArgs e)
        {
            M.WriteMemory(UBrgUIManager.AActor.mDurabilityDownDisable, "int", "64");
        }

        private void InfiniteDurability_Toggle_CheckedChanged(object sender, EventArgs e)
        {
            if (InfiniteDurability_Toggle.Checked)
            {
                InfiniteDurability_Label.Text = "- INFINITE DURABILITY: ENABLED";
                InfiniteDurability_Timer.Start();
            }
            else
            {
                InfiniteDurability_Label.Text = "- INFINITE DURABILITY: DISABLED";
                InfiniteDurability_Timer.Stop();
                M.WriteMemory(UBrgUIManager.AActor.mDurabilityDownDisable, "int", "32");
            }
        }

        private void SpeedhackWalk_Toggle_CheckedChanged(object sender, EventArgs e)
        {
            if (SpeedhackWalk_Toggle.Checked)
            {
                M.WriteMemory(UBrgUIManager.ABrgPawn_BaseNative.mRunSpeedPerSecond, "float", "3000");
                SpeedhackWalk_Label.Text = "- SPEEDHACK (WALK): ENABLED";
            }
            else
            {
                M.WriteMemory(UBrgUIManager.ABrgPawn_BaseNative.mRunSpeedPerSecond, "float", "540");
                SpeedhackWalk_Label.Text = "- SPEEDHACK (WALK): DISABLED";
            }
        }

        private void SpeedhackSprint_Toggle_CheckedChanged(object sender, EventArgs e)
        {
            if (SpeedhackSprint_Toggle.Checked)
            {
                M.WriteMemory(UBrgUIManager.ABrgPawn_BaseNative.mDashSpeedPerSecond, "float", "3000");
                SpeedhackSprint_Label.Text = "- SPEEDHACK (SPRINT): ENABLED";
            }
            else
            {
                M.WriteMemory(UBrgUIManager.ABrgPawn_BaseNative.mDashSpeedPerSecond, "float", "850");
                SpeedhackSprint_Label.Text = "- SPEEDHACK (SPRINT): DISABLED";
            }
        }

        private void SpeedhackCarryWalk_Toggle_CheckedChanged(object sender, EventArgs e)
        {
            if (SpeedhackCarryWalk_Toggle.Checked)
            {
                M.WriteMemory(UBrgUIManager.ABrgPawn_BaseNative.mCarryWalkSpeedPerSecond, "float", "3000");
                SpeedhackCarryWalk_Label.Text = "- SPEEDHACK (CARRY WALK): ENABLED";
            }
            else
            {
                M.WriteMemory(UBrgUIManager.ABrgPawn_BaseNative.mCarryWalkSpeedPerSecond, "float", "150");
                SpeedhackCarryWalk_Label.Text = "- SPEEDHACK (CARRY WALK): DISABLED";
            }
        }

        private void SpeedhackCarrySprint_Toggle_CheckedChanged(object sender, EventArgs e)
        {
            if (SpeedhackCarrySprint_Toggle.Checked)
            {
                M.WriteMemory(UBrgUIManager.ABrgPawn_BaseNative.mCarryRunSpeedPerSecond, "float", "3000");
                SpeedhackCarrySprint_Label.Text = "- SPEEDHACK (CARRY SPRINT): ENABLED";
            }
            else
            {
                M.WriteMemory(UBrgUIManager.ABrgPawn_BaseNative.mCarryRunSpeedPerSecond, "float", "450");
                SpeedhackCarrySprint_Label.Text = "- SPEEDHACK (CARRY SPRINT): DISABLED";
            }
        }
        private void SuperJump_Toggle_CheckedChanged(object sender, EventArgs e)
        {
            if (SuperJump_Toggle.Checked)
            {
                M.WriteMemory(UBrgUIManager.ABrgPawn_Base.mJumpStartPower, "float", "3000");
                SuperJump_Label.Text = "- SUPER JUMP: ENABLED";
            }
            else
            {
                M.WriteMemory(UBrgUIManager.ABrgPawn_Base.mJumpStartPower, "float", "1100");
                SuperJump_Label.Text = "- SUPER JUMP: DISABLED";
            }
        }

        private void OneHitKill_Toggle_CheckedChanged(object sender, EventArgs e)
        {
            if (OneHitKill_Toggle.Checked)
            {
                M.WriteMemory(UBrgUIManager.ABrgCommonPawn_CustomChara.mBaseAtkUpScale, "float", "1000000");
                OneHitKill_Label.Text = "- 1 HIT KILL: ENABLED";
            }
            else
            {
                M.WriteMemory(UBrgUIManager.ABrgCommonPawn_CustomChara.mBaseAtkUpScale, "float", "1");
                OneHitKill_Label.Text = "- 1 HIT KILL: DISABLED";
            }
        }

        private void NoRecoil_Timer_Tick(object sender, EventArgs e)
        {
            if (NoRecoil_Timer.Enabled)
            {
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
                M.WriteMemory(UBrgUIManager.PlayerCtrlCustomChara.mRecoil, "byte", "0");
                M.WriteMemory(UBrgUIManager.ABrgCommonPawn_CustomCharaNative.FBrgSkillStatus.RecoilDownRate, "float", "0");
                M.WriteMemory(UBrgUIManager.ABrgCommonPawn_CustomCharaNative.FBrgSkillStatus.LessDiffusionRate, "float", "0");
                M.WriteMemory(UBrgUIManager.ABrgCommonPawn_CustomCharaNative.FBrgSkillStatus.LessDiffusionRateRevolver, "float", "0");
                M.WriteMemory(UBrgUIManager.ABrgCommonPawn_CustomCharaNative.FBrgSkillStatus.LessDiffusionRateShotGun, "float", "0");
                M.WriteMemory(UBrgUIManager.ABrgCommonPawn_CustomCharaNative.FBrgSkillStatus.RecoilDownRate, "float", "0");
            }
        }

        private void NoRecoil_Toggle_CheckedChanged(object sender, EventArgs e)
        {
            if (NoRecoil_Toggle.Checked)
            {
                NoRecoil_Timer.Start();
                NoRecoil_Label.Text = "- NO RECOIL: ENABLED";
            }
            else
            {
                NoRecoil_Timer.Stop();
                NoRecoil_Label.Text = "- NO RECOIL: DISABLED";
            }
        }

        private void InfiniteAmmo_Toggle_CheckedChanged(object sender, EventArgs e)
        {
            if (InfiniteAmmo_Toggle.Checked)
            {
                M.WriteMemory(UBrgUIManager.ABrgPawn_BaseNative.mGunBulletConsumptionDisable, "byte", "08");
                InfiniteAmmo_Label.Text = "- INFINITE AMMO: ENABLED";
            }
            else
            {
                M.WriteMemory(UBrgUIManager.ABrgPawn_BaseNative.mGunBulletConsumptionDisable, "byte", "00");
                InfiniteAmmo_Label.Text = "- INFINITE AMMO: DISABLED";
            }
        }

        private void TPose_Timer_Tick(object sender, EventArgs e)
        {
            M.WriteMemory(UBrgUIManager.ABrgGameInfoNative.ABrgPawn_Base.PlayerBase.UBrgSkeletalMeshComponent.bForceRefpose, "int", "1");
        }

        private void TPose_Toggle_CheckedChanged(object sender, EventArgs e)
        {
            if (TPose_Toggle.Checked)
            {
                TPose_Timer.Enabled = true;
                TPose_Label.Text = "- T POSE: ENABLED";
            }
            else
            {
                TPose_Timer.Enabled = false;
                TPose_Label.Text = "- T POSE: DISABLED";
                M.WriteMemory(UBrgUIManager.ABrgGameInfoNative.ABrgPawn_Base.PlayerBase.UBrgSkeletalMeshComponent.bForceRefpose, "int", "0");
            }
        }

        private void NoFallDamage_Toggle_CheckedChanged(object sender, EventArgs e)
        {
            if (NoFallDamage_Toggle.Checked)
            {
                M.WriteMemory(UBrgUIManager.ABrgPawn_BaseNative.mFallDamageStartHeight, "float", "99999999");
                NoFallDamage_Label.Text = "- NO FALL DAMAGE: ENABLED";
            }
            else
            {
                M.WriteMemory(UBrgUIManager.ABrgPawn_BaseNative.mFallDamageStartHeight, "float", "600");
                NoFallDamage_Label.Text = "- NO FALL DAMAGE: DISABLED";
            }
        }

        private void UnFogMiniMap_Timer_Tick(object sender, EventArgs e)
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

        private void UnFogMiniMap_Toggle_CheckedChanged(object sender, EventArgs e)
        {
            if (UnFogMiniMap_Toggle.Checked)
            {
                UnFogMiniMap_Timer.Start();
                UnFogMiniMap_Label.Text = "- UN FOG MINI MAP: ENABLED";
            }
            else
            {
                UnFogMiniMap_Timer.Stop();
                M.WriteMemory(UBrgUIManager.UBrgUIMiniMapManager.mMiniMapSpraySizeScale, "float", "90");
                UnFogMiniMap_Label.Text = "- UN FOG MINI MAP: DISABLED";
            }
        }
        private void KillcoinVacuum_Timer_Tick(object sender, EventArgs e)
        {
            M.WriteMemory(UBrgUIManager.ABrgGameInfoNative.mMoneyVacuumFrame, "float", "999999");
        }

        private void KillcoinVacuum_Toggle_CheckedChanged(object sender, EventArgs e)
        {
            if (KillcoinVacuum_Toggle.Checked)
            {
                KillcoinVacuum_Label.Text = "- KILLCOIN VACUUM: ENABLED";
                KillcoinVacuum_Timer.Start();
            }
            else
            {
                KillcoinVacuum_Label.Text = "- KILLCOIN VACUUM: DISABLED";
                M.WriteMemory(UBrgUIManager.ABrgGameInfoNative.mMoneyVacuumFrame, "float", "0");
                KillcoinVacuum_Timer.Stop();
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
            if (OpenDailyRewardBox_Toggle.Checked)
            {
                OpenDailyRewardBox_Label.Text = "- OPEN DAILY REWARD BOX: ENABLED";
                M.WriteMemory(UBrgUIManager.ABrgGameInfoNative.DailyRewardBox.mbOpen, "int", "38");
                // Start the timer
                OpenDailyRewardBox_Timer.Start();
            }
            else
            {
                OpenDailyRewardBox_Label.Text = "- OPEN DAILY REWARD BOX: DISABLED";
                // Stop the timer if the toggle is manually disabled before the timer ticks
                OpenDailyRewardBox_Timer.Stop();
            }
        }
        #endregion

        public void UpdateElevatorPosition()
        {
            float elevator1X = M.ReadFloat(UBrgUIManager.ABrgGameInfoNativeBase.ElevatorLocations.ElevatorLocation1_X);
            float elevator1Y = M.ReadFloat(UBrgUIManager.ABrgGameInfoNativeBase.ElevatorLocations.ElevatorLocation1_Y);
            float elevator1Z = M.ReadFloat(UBrgUIManager.ABrgGameInfoNativeBase.ElevatorLocations.ElevatorLocation1_Z);

            if (!IsDefaultOrZero(elevator1X))
            {
                UpdatePlayerLocation(elevator1X, elevator1Y, elevator1Z);
            }
        }

        private bool IsDefaultOrZero(float value)
        {
            return value == 0f;
        }

        private void UpdatePlayerLocation(float x, float y, float z)
        {
            M.WriteMemory(UBrgUIManager.ABrgGameInfoNative.ABrgPawn_Base.PlayerBase.Location_X, "float", x.ToString(CultureInfo.InvariantCulture));
            M.WriteMemory(UBrgUIManager.ABrgGameInfoNative.ABrgPawn_Base.PlayerBase.Location_Y, "float", y.ToString(CultureInfo.InvariantCulture));
            M.WriteMemory(UBrgUIManager.ABrgGameInfoNative.ABrgPawn_Base.PlayerBase.Location_Z, "float", z.ToString(CultureInfo.InvariantCulture));
        }

    }
}
