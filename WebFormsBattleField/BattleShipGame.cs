using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebFormsBattleField
{
    public class BattleShipGame 
    {
        private BattleShipUI UI = new BattleShipUI();
        ShipCollection playerShips = new ShipCollection("Player");
        ShipCollection pcShips = new ShipCollection("Pc");
        Random rnd = new Random();

        private Page GamePage { get; set; }

        private List<string> PlayerMoveHistory
        {
            get { return (List<string>)HttpContext.Current.Session["PlayerMoveHistory"]; }
            set { HttpContext.Current.Session["PlayerMoveHistory"] = value; }
        }
        private List<string> PcMoveHistory
        {
            get { return (List<string>)HttpContext.Current.Session["PcMoveHistory"]; }
            set { HttpContext.Current.Session["PcMoveHistory"] = value; }
        }

        private List<int> PcMoveSelection
        {
            get { return (List<int>)HttpContext.Current.Session["PcMoveSelection"]; }
            set { HttpContext.Current.Session["PcMoveSelection"] = value; }
        }
        private List<int> PcCurrentHitList
        {
            get { return (List<int>)HttpContext.Current.Session["PcCurrentHitList"]; }
            set { HttpContext.Current.Session["PcCurrentHitList"] = value; }
        }
        private List<int> PcCurrentHitEdgeList
        {
            get { return (List<int>)HttpContext.Current.Session["PcCurrentHitEdgeList"]; }
            set { HttpContext.Current.Session["PcCurrentHitEdgeList"] = value; }
        }
        private bool PcIsShipDestroyed
        {
            get { return (bool)HttpContext.Current.Session["PcIsShipDestroyed"]; }
            set { HttpContext.Current.Session["PcIsShipDestroyed"] = value; }
        }
        private bool PcUpOrDown
        {
            get { return (bool)HttpContext.Current.Session["PcUpOrDown"]; }
            set { HttpContext.Current.Session["PcUpOrDown"] = value; }
        }
        private bool PcLeftOrRight
        {
            get { return (bool)HttpContext.Current.Session["PcLeftOrRight"]; }
            set { HttpContext.Current.Session["PcLeftOrRight"] = value; }
        }
        private int? PcCurrentPick
        {
            get { return (int?)HttpContext.Current.Session["PcCurrentPick"]; }
            set { HttpContext.Current.Session["PcCurrentPick"] = value; }
        }
        private int? PcPastPick
        {
            get { return (int?)HttpContext.Current.Session["PcPastPick"]; }
            set { HttpContext.Current.Session["PcPastPick"] = value; }
        }
        private int PcScore
        {
            get { return (int)HttpContext.Current.Session["PcScore"]; }
            set { HttpContext.Current.Session["PcScore"] = value; }
        }
        private int PlayerScore
        {
            get { return (int)HttpContext.Current.Session["PlayerScore"]; }
            set { HttpContext.Current.Session["PlayerScore"] = value; }
        }

        public BattleShipGame(Page page)
        {
            GamePage = page;
        }

        public void Initialize()
        {
            CreateUI();

            if(!GamePage.IsPostBack)
            {
                InitializeStates();
            }
            UpdateHistory();
        }

        private void CreateUI()
        {
            UI.Create(GamePage.Form);
            UI.SetUIButtonsEventListener(StartButton_Click, Reset_Click, ManualButton_Click, AutomaticButton_Click);
            UI.InitializeCells(Cell_Click);
        }

        private void InitializeStates()
        {
            PlayerMoveHistory = new List<string>();
            PcMoveHistory = new List<string>();

            PcScore = 0;
            PlayerScore = 0;

            PcCurrentHitList = new List<int>();
            PcCurrentHitEdgeList = new List<int>();
            PcIsShipDestroyed = true;
            PcUpOrDown = true;
            PcLeftOrRight = true;
            PcCurrentPick = null;
            PcPastPick = null;

            PcMoveSelection = new List<int>();
            for (int i = 0; i < 100; i++)
            {
                PcMoveSelection.Add(i);
            }
        }

        private void UpdateHistory()
        {
            foreach (string val in PlayerMoveHistory)
            {
                UI.AddHistory(val, true, false);
            }

            foreach (string val in PcMoveHistory)
            {
                UI.AddHistory(val, false, true);
            }
        }

        private void SetGameState(string gameTitle, string info)
        {
            UI.SetGameStateUI(gameTitle, info);
            ShowPcShips();
        }

        private void ShowPcShips()
        {
            string script = "window.onload = function() { showPcShips(); };";
            GamePage.ClientScript.RegisterStartupScript(this.GetType(), "showPcShips", script, true);
        }

        private void PcMove()
        {
            int randomPick = RandomGenerator();

            var targetCell = UI.PlayerCells[randomPick];
            targetCell.CssClass += " targeted";

            var buttonCell = (Button)targetCell.Controls[0];

            if (targetCell.CssClass.Contains("taken"))
            {
                PcScore++;

                foreach (Ship ship in playerShips.ShipsList)
                {
                    if (targetCell.CssClass.Contains(ship.Name)) { ship.Hit++; }
                }

                PcIsShipDestroyed = false;
                targetCell.CssClass += " hitted";
                buttonCell.CssClass += " hit";
                buttonCell.BackColor = Color.Red;
                PcMoveHistory.Add("Hit");
                UI.AddHistory("Hit", false, true);

                PcCurrentHitEdgeList.Add(randomPick);
                PcCurrentHitList.Add(randomPick);
                PcCurrentPick = randomPick;

                if (PcPastPick != null)
                {
                    if (Math.Abs((int)(PcCurrentPick - PcPastPick)) == 1)
                    {
                        PcLeftOrRight = true;
                        PcUpOrDown = false;
                    }
                    else if (Math.Abs((int)(PcCurrentPick - PcPastPick)) == 10)
                    {
                        PcLeftOrRight = false;
                        PcUpOrDown = true;
                    }
                }

                PcPastPick = PcCurrentPick;

                RemoveInBetweenDamagedCell();
            }
            else
            {
                targetCell.CssClass += " missed";
                buttonCell.CssClass += " miss";
                buttonCell.BackColor = Color.White;
                PcMoveHistory.Add("Miss");
                UI.AddHistory("Miss", false, true);
            }

            IsShipDestroyed();

            bool isPcWin = PcScore == 20;

            if (isPcWin)
            {
                SetGameState("PC WIN!", "Better Luck Next Time!");
            }

            bool isLargestShipDestroyed = PcCurrentHitList.Count == 4;

            if (PcIsShipDestroyed || isLargestShipDestroyed)
            {
                PcLeftOrRight = true;
                PcUpOrDown = true;
                PcPastPick = null;
                PcIsShipDestroyed = true;

                PcCurrentHitList.ForEach(destroyedCell =>
                {
                    UI.PlayerCells[destroyedCell].CssClass += " destroyed";

                    RemoveSurroundingCells(destroyedCell);
                });

                PcCurrentHitEdgeList.Clear();
                PcCurrentHitList.Clear();

            }
        }

        private void RemoveSurroundingCells(int cell)
        {
            //remove left cell
            RemoveCell(cell > 0 && cell % 10 != 0, cell - 1);
            //remove right cell
            RemoveCell(cell < 99 && cell % 10 != 9, cell + 1);
            //remove up cell
            RemoveCell(cell > 9, cell - 10);
            //remove down cell
            RemoveCell(cell < 90, cell + 10);
            //remove upper left cell
            RemoveCell(cell > 10 && cell % 10 != 0, cell - 11);
            //remove upper right cell
            RemoveCell(cell > 9 && cell % 10 != 9, cell - 9);
            //remove lower left cell
            RemoveCell(cell < 90 && cell % 10 != 0, cell + 9);
            //remove lower right cell 
            RemoveCell(cell < 89 && cell % 10 != 9, cell + 11);
        }

        private void RemoveCell(bool isTargetValid, int target)
        {
            if (isTargetValid && PcMoveSelection.Contains(target))
            {
                PcMoveSelection.Remove(target);
                UI.PlayerCells[target].CssClass += " removed";
            }
        }

        private void RemoveInBetweenDamagedCell()
        {
            int? inBetweenCell = null;
            PcCurrentHitEdgeList.ForEach(hitCell =>
            {
                if (hitCell % 10 != 0 &&
                    hitCell % 10 != 9 &&
                    UI.PlayerCells[hitCell + 1].CssClass.Contains("hitted") &&
                    UI.PlayerCells[hitCell - 1].CssClass.Contains("hitted"))
                {
                    inBetweenCell = hitCell;
                }

                if (hitCell > 9 &&
                    hitCell < 90 &&
                    UI.PlayerCells[hitCell + 10].CssClass.Contains("hitted") &&
                    UI.PlayerCells[hitCell - 10].CssClass.Contains("hitted"))
                {
                    inBetweenCell = hitCell;
                }
            });

            if (inBetweenCell != null)
            {
                PcCurrentHitEdgeList.Remove((int)inBetweenCell);
            }
        }

        private void IsShipDestroyed()
        {
            if (PcLeftOrRight && PcUpOrDown)
            {
                PcIsShipDestroyed = IsUpAndDownMarked() && IsLeftAndRightMarked();
            }
            else if (PcLeftOrRight)
            {
                PcIsShipDestroyed = IsLeftAndRightMarked();
            }
            else if (PcUpOrDown)
            {
                PcIsShipDestroyed = IsUpAndDownMarked();
            }
        }

        private bool IsUpAndDownMarked()
        {
            bool isUpMarked = PcCurrentHitEdgeList.All(item => IsSideMarked(item > 9, item - 10));
            bool isDownMarked = PcCurrentHitEdgeList.All(item => IsSideMarked(item < 90, item + 10));
            return isUpMarked && isDownMarked;
        }

        private bool IsLeftAndRightMarked()
        {
            bool isLeftMarked = PcCurrentHitEdgeList.All(item => IsSideMarked(item % 10 != 0, item - 1));
            bool isRightMarked = PcCurrentHitEdgeList.All(item => IsSideMarked(item % 10 != 9, item + 1));
            return isLeftMarked && isRightMarked;
        }

        private bool IsSideMarked(bool isPositionValid, int position)
        {
            if (isPositionValid)
            {
                return
                    UI.PlayerCells[position].CssClass.Contains("missed") ||
                    UI.PlayerCells[position].CssClass.Contains("hitted") ||
                    UI.PlayerCells[position].CssClass.Contains("removed");
            }
            return true;
        }

        private int RandomGenerator()
        {
            int newPick;
            while (!PcIsShipDestroyed)
            {
                int randomDirection = 0;

                if (PcUpOrDown && PcLeftOrRight)
                {
                    randomDirection = rnd.Next(0, 4);
                }
                else if (PcUpOrDown)
                {
                    randomDirection = rnd.Next(0, 2) + 2;
                }
                else if (PcLeftOrRight)
                {
                    randomDirection = rnd.Next(0, 2);
                }

                newPick = PcCurrentHitEdgeList[rnd.Next(0, PcCurrentHitEdgeList.Count)];

                switch (randomDirection)
                {
                    case 0:
                        if (newPick % 10 != 9) newPick++;

                        break;
                    case 1:
                        if (newPick % 10 != 0) newPick--;

                        break;
                    case 2:
                        if (newPick > 9) newPick -= 10;

                        break;
                    case 3:
                        if (newPick < 90) newPick += 10;

                        break;
                }

                if (PcMoveSelection.Contains(newPick))
                {
                    PcMoveSelection.Remove(newPick);
                    return newPick;
                }
            }

            newPick = PcMoveSelection[rnd.Next(0, PcMoveSelection.Count)];
            PcMoveSelection.Remove(newPick);
            return newPick;
        }

        private void Generate(Ship ship, List<Panel> cells, Random rnd)
        {
            int direction;

            int randomDirection = rnd.Next(0, ship.Directions.Count);
            var current = ship.Directions[randomDirection];
            if (randomDirection == 0) { direction = 1; }
            else { direction = 10; }

            int randomStart = rnd.Next(0, (cells.Count - (current.Count * direction)));

            bool isTaken = current.Any((item) => cells[randomStart + item].CssClass.Contains("taken"));
            bool isAtRightEdge = current.Any((item) => (randomStart + item) % 10 == 9);
            bool isAtLeftEdge = current.Any((item) => (randomStart + item) % 10 == 0);
            bool isSurroundingsTaken = IsSurroundingsTaken(current, cells, randomStart);

            if (!isTaken && !(isAtLeftEdge && isAtRightEdge) && !isSurroundingsTaken)
            {
                current.ForEach(
                    (item) => cells[randomStart + item].CssClass += string.Format(" taken {0}{1}", ship.Owner, ship.Name)
                );
            }
            else
            {
                Generate(ship, cells, rnd);
            }
        }

        private bool IsSurroundingsTaken(List<int> current, List<Panel> cells, int randomStart)
        {
            bool isUpperTaken = current.Any((item) =>
                randomStart + item > 9 && cells[randomStart + item - 10].CssClass.Contains("taken")
            );

            bool isLowerTaken = current.Any((item) =>
                randomStart + item < 90 && cells[randomStart + item + 10].CssClass.Contains("taken")
            );

            bool isLeftTaken = current.Any((item) =>
                (randomStart + item) % 10 != 0 && cells[randomStart + item - 1].CssClass.Contains("taken")
            );

            bool isRightTaken = current.Any((item) =>
                (randomStart + item) % 10 != 9 && cells[randomStart + item + 1].CssClass.Contains("taken")
            );

            bool isUpperLeftTaken = current.Any((item) =>
                randomStart + item > 10 &&
                (randomStart + item) % 10 != 0 &&
                cells[randomStart + item - (10 + 1)].CssClass.Contains("taken")
            );

            bool isUpperRightTaken = current.Any((item) =>
                randomStart + item > 9 &&
                (randomStart + item) % 10 != 9 &&
                cells[randomStart + item - (10 - 1)].CssClass.Contains("taken")
            );

            bool isLowerLeftTaken = current.Any((item) =>
                randomStart + item < 90 &&
                (randomStart + item) % 10 != 0 &&
                cells[randomStart + item + (10 - 1)].CssClass.Contains("taken")
            );

            bool isLowerRightTaken = current.Any((item) =>
                randomStart + item < 89 &&
                (randomStart + item) % 10 != 9 &&
                cells[randomStart + item + (10 + 1)].CssClass.Contains("taken")
            );

            return
                isUpperTaken ||
                isLowerTaken ||
                isLeftTaken ||
                isRightTaken ||
                isUpperLeftTaken ||
                isUpperRightTaken ||
                isLowerLeftTaken ||
                isLowerRightTaken;
        }

        private void Cell_Click(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            Panel cell = (Panel)button.Parent;

            if (cell.CssClass.Contains("taken"))
            {
                button.CssClass += " hit";
                button.BackColor = Color.Red;
                PlayerScore++;
                PlayerMoveHistory.Add("Hit");
                UI.AddHistory("Hit", true, false);
            }
            else
            {
                button.CssClass += " miss";
                button.BackColor = Color.White;
                PlayerMoveHistory.Add("Miss");
                UI.AddHistory("Miss", true, false);
            }
            button.Enabled = false;

            bool isPlayerWin = PlayerScore == 20;

            if (isPlayerWin)
            {
                SetGameState("YOU WIN!", "Congratulations!");
            }
            else
            {
                PcMove();
            }
        }

        protected void Reset_Click(object sender, EventArgs e)
        {
            HttpContext.Current.Response.Redirect(HttpContext.Current.Request.RawUrl);
        }

        protected void AutomaticButton_Click(object sender, EventArgs e)
        {
            playerShips.ShipsList.ForEach((ship) =>
            {
                Generate(ship, UI.PlayerCells, rnd);
            });

            pcShips.ShipsList.ForEach((ship) =>
            {
                Generate(ship, UI.PcCells, rnd);
            });

            UI.AutomaticButtonClicked();
        }

        protected void StartButton_Click(object sender, EventArgs e)
        {
            pcShips.ShipsList.ForEach((ship) =>
            {
                Generate(ship, UI.PcCells, rnd);
            });

            string[] ManualShipsGeneratorData = UI.GetManualShipsGeneratorData().Split(';');

            foreach (string shipData in ManualShipsGeneratorData)
            {
                string[] data = shipData.Split(',');

                Panel cell = (Panel)GamePage.FindControl(data[0]);
                if (cell != null) cell.CssClass += string.Format(" taken {0}", data[1]);
            }

            UI.StartButtonClicked();
        }

        protected void ManualButton_Click(object sender, EventArgs e)
        {
            string script = "window.onload = function() { manualMode(); };";
            GamePage.ClientScript.RegisterStartupScript(this.GetType(), "manualMode", script, true);

            UI.ManualButtonClicked();
        }
    }
}