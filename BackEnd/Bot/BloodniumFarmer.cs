using KC__LID_EXT.BackEnd.Dump;

namespace STARFIRE.BackEnd.Bot
{
    public class BloodniumFarmer
    {
        private bool isRunning = false;
        
        private readonly LetItDie _lid;

        public BloodniumFarmer(LetItDie letItDie)
        {
            _lid = letItDie;
        }

        public void Toggle()
        {
            if (isRunning)
            {
                End();
            }
            else
            {
                Start();
            }
        }

        public void End()
        {
            TeleportBackToWaitingRoom();
        }

        private void TeleportBackToWaitingRoom()
        {
            // teleport to waiting room
            // wait fore Due to an unexpecte error text to appear
            // press enter
        }

        public void Start()
        {
            TeleportToKcElevator();
            SelectFloor51();
            TeleportToFrontGate();
        }
        

        private void TeleportToFrontGate()
        {
            
            
            // press F Key
            // Press Enter
            // Press Enter
            // Wait a long time
            // maybe wait for Tengokumon text to appear?
        }

        private void SelectFloor51()
        {
            // press F Key
            // Press Enter
            // Press Enter
            // Wait a long time
            // maybe wait for Tengokumon text to appear?
        }

        private void TeleportToKcElevator()
        {
            _lid.TeleportToRegularElevator();   
        }
    }
}