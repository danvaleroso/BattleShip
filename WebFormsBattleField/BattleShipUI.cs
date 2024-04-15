using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Drawing;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace WebFormsBattleField
{
    public class BattleShipUI : System.Web.UI.Page
    {
        Panel InfoContainer;
        Label GameTitle;
        Label Info;

        Panel Container;

        Panel PlayerHistory;
        HtmlGenericControl PlayerHistoryList;

        Panel PlayerFieldContainer;
        Panel PlayerFieldPanel;

        Panel PcFieldContainer;
        Panel PcFieldPanel;

        Panel ShipsDisplayContainer;
        Panel ShipsDisplay;

        Panel PcHistory;
        HtmlGenericControl PcHistoryList;

        Panel ButtonControls;
        Button AutomaticButton;
        Button StartButton;
        Button ManualButton;
        Button PlayAgainButton;
        HiddenField ManualShipsGenerator;

        public List<Panel> PlayerCells { get; set; }
        public List<Panel> PcCells { get; set; }

        public void Create(Control thisForm)
        {
            CreateInfoContainer();
            CreateGameContainer();
            CreateButtonControls();

            InitializeUI();
            OnPageRequestUI();

            thisForm.Controls.Add(InfoContainer);
            thisForm.Controls.Add(Container);
            thisForm.Controls.Add(ButtonControls);
        }

        public void InitializeCells(EventHandler clickEvent)
        {
            PlayerCells = CreateCells(PlayerFieldPanel, "PlayerCells", false, clickEvent);
            PcCells = CreateCells(PcFieldPanel, "PcCells", true, clickEvent);
        }

        public string GetManualShipsGeneratorData()
        {
            return ManualShipsGenerator.Value;
        }

        public void SetUIButtonsEventListener(EventHandler StartButtonEventHandler, EventHandler PlayAgainButtonEventHandler, EventHandler ManualButtonEventHandler, EventHandler AutomaticButtonEventHandler)
        {
            StartButton.Click += StartButtonEventHandler;
            PlayAgainButton.Click += PlayAgainButtonEventHandler;
            ManualButton.Click += ManualButtonEventHandler;
            AutomaticButton.Click += AutomaticButtonEventHandler;
        }

        public void SetGameStateUI(string gameTitle, string info)
        {
            GameTitle.Text = gameTitle;
            Info.Text = info;
            PcFieldPanel.Enabled = false;
            PlayAgainButton.Visible = true;
        }

        public void AddHistory(string history, bool player, bool pc)
        {
            HtmlGenericControl li = new HtmlGenericControl("li");
            li.InnerText = history;
            if (player) PlayerHistoryList.Controls.AddAt(0, li);
            if (pc) PcHistoryList.Controls.AddAt(0, li);
        }

        public void AutomaticButtonClicked()
        {
            PcFieldPanel.Enabled = true;
            AutomaticButton.Visible = false;
            ManualButton.Visible = false;
            Info.Text = "Playing";
        }

        public void StartButtonClicked()
        {
            ShipsDisplayContainer.Visible = false;
            PcFieldContainer.Visible = true;
            PcFieldPanel.Enabled = true;
            StartButton.Visible = false;
            ManualShipsGenerator.Value = "";
            Info.Text = "Playing";
        }

        public void ManualButtonClicked()
        {
            PcFieldContainer.Visible = false;
            ShipsDisplayContainer.Visible = true;

            AutomaticButton.Visible = false;
            ManualButton.Visible = false;
            StartButton.Visible = true;

            Info.Text = "Click Ships to Rotate";
        }

        private List<Panel> CreateCells(Panel FieldArea, string cellId, bool isClickable, EventHandler clickEvent)
        {
            for (int i = 0; i < 100; i++)
            {
                var cell = new Panel
                {
                    ID = cellId + i.ToString(),
                    CssClass = "field-cell"
                };

                var button = new Button
                {
                    ID = cellId + "Button" + i.ToString(),
                    CssClass = "field-cell-button",
                    BackColor = Color.Transparent
                };

                if (isClickable)
                {
                    button.Click += clickEvent;
                }
                else
                {
                    button.Enabled = false;
                }

                cell.Controls.Add(button);
                FieldArea.Controls.Add(cell);
            }

            return FieldArea.Controls.OfType<Panel>().ToList();
        }

        private void OnPageRequestUI()
        {
            if (!IsPostBack)
            {
                PcFieldPanel.Enabled = false;
                ShipsDisplayContainer.Visible = false;
                PlayAgainButton.Visible = false;
                StartButton.Visible = false;
            }
        }

        private void CreateInfoContainer()
        {
            InfoContainer = new Panel();
            GameTitle = new Label();
            Info = new Label();

            InfoContainer.Controls.Add(GameTitle);
            InfoContainer.Controls.Add(new LiteralControl("<br />"));
            InfoContainer.Controls.Add(Info);
        }

        private void CreateGameContainer()
        {
            Container = new Panel();

            CreatePlayerHistoryContainer();
            CreatePlayerFieldContainer();
            CreatePcFieldContainer();
            CreateShipsDisplayContainer();
            CreatePcHistoryContainer();
        }

        private void CreatePlayerHistoryContainer()
        {
            PlayerHistory = new Panel();
            PlayerHistoryList = new HtmlGenericControl("ol");
            PlayerHistory.Controls.Add(new HtmlGenericControl("h4") { InnerHtml = "PLAYER's History" });
            PlayerHistory.Controls.Add(PlayerHistoryList);

            Container.Controls.Add(PlayerHistory);
        }

        private void CreatePlayerFieldContainer()
        {
            PlayerFieldContainer = new Panel();
            PlayerFieldPanel = new Panel();

            PlayerFieldContainer.Controls.Add(new HtmlGenericControl("h2") { InnerHtml = "PLAYER'S FIELD" });
            PlayerFieldContainer.Controls.Add(PlayerFieldPanel);

            Container.Controls.Add(PlayerFieldContainer);
        }

        private void CreatePcFieldContainer()
        {
            PcFieldContainer = new Panel();
            PcFieldPanel = new Panel();
            PcFieldContainer.Controls.Add(new HtmlGenericControl("h2") { InnerHtml = "PC'S FIELD" });
            PcFieldContainer.Controls.Add(PcFieldPanel);

            Container.Controls.Add(PcFieldContainer);
        }

        private void CreateShipsDisplayContainer()
        {
            ShipsDisplayContainer = new Panel();
            ShipsDisplay = new Panel();
            ShipsDisplay.Controls.Add(CreateShipContainer("Submarine", "ship Submarine-container", 4, 1));
            ShipsDisplay.Controls.Add(CreateShipContainer("Destroyer", "ship Destroyer-container", 3, 2));
            ShipsDisplay.Controls.Add(CreateShipContainer("Cruiser", "ship Cruiser-container", 2, 3));
            ShipsDisplay.Controls.Add(CreateShipContainer("Battleship", "ship Battleship-container", 1, 4));

            ShipsDisplayContainer.Controls.Add(new HtmlGenericControl("h2") { InnerHtml = "Drag and Drop Ships" });
            ShipsDisplayContainer.Controls.Add(ShipsDisplay);

            Container.Controls.Add(ShipsDisplayContainer);
        }

        private Panel CreateShipContainer(string id, string cssClass, int count, int value)
        {
            Panel ShipContainer = new Panel
            {
                CssClass = "ship-container"
            };

            HtmlGenericControl ship = new HtmlGenericControl("div");
            ship.ID = id;
            ship.Attributes.Add("class", cssClass);
            ship.Attributes.Add("draggable", "true");
            ship.Attributes.Add("data-count", count.ToString());
            ship.Attributes.Add("data-ishorizontal", "true");

            for (int i = 0; i < value; i++)
            {
                HtmlGenericControl shipCell = new HtmlGenericControl("div");
                shipCell.Attributes.Add("data-value", i.ToString());

                ship.Controls.Add(shipCell);
            }

            ShipContainer.Controls.Add(ship);
            ShipContainer.Controls.Add(new LiteralControl("<p>x </p>"));
            ShipContainer.Controls.Add(new HtmlGenericControl("h4") { InnerText = count.ToString() });

            return ShipContainer;
        }

        private void CreatePcHistoryContainer()
        {
            PcHistory = new Panel();
            PcHistoryList = new HtmlGenericControl("ol");
            PcHistory.Controls.Add(new HtmlGenericControl("h4") { InnerHtml = "PC's History" });
            PcHistory.Controls.Add(PcHistoryList);

            Container.Controls.Add(PcHistory);
        }

        private void CreateButtonControls()
        {
            ButtonControls = new Panel();
            AutomaticButton = new Button();
            StartButton = new Button();
            ManualButton = new Button();
            PlayAgainButton = new Button();
            ManualShipsGenerator = new HiddenField();

            ButtonControls.Controls.Add(AutomaticButton);
            ButtonControls.Controls.Add(ManualButton);
            ButtonControls.Controls.Add(StartButton);
            ButtonControls.Controls.Add(PlayAgainButton);
            ButtonControls.Controls.Add(ManualShipsGenerator);
        }

        private void InitializeUI()
        {
            InfoContainer.CssClass = "info-container";

            GameTitle.ID = "GameTitle";
            GameTitle.Text = "BATTLE SHIP";

            Info.ID = "Info";
            Info.Text = "Select Ships Placing Mode";

            Container.CssClass = "container";

            PlayerHistory.CssClass = "history-player";
            PlayerHistory.ID = "PlayerHistory";
            PlayerHistoryList.ID = "PlayerHistoryList";
            PlayerHistoryList.Attributes.Add("runat", "server");
            PlayerHistoryList.Attributes.Add("reversed", "reversed");

            PlayerFieldContainer.CssClass = "player-field";

            PlayerFieldPanel.ID = "PlayerFieldPanel";
            PlayerFieldPanel.CssClass = "grid grid-player";

            PcFieldContainer.CssClass = "pc-field";

            PcFieldPanel.ID = "PcFieldPanel";
            PcFieldPanel.CssClass = "grid grid-pc";

            ShipsDisplayContainer.ID = "ShipsDisplayContainer";
            ShipsDisplayContainer.CssClass = "display-field";
            ShipsDisplay.ID = "ShipsDisplay";
            ShipsDisplay.CssClass = "grid-display";

            PcHistory.CssClass = "history-pc";
            PcHistory.ID = "PcHistory";
            PcHistoryList.ID = "PcHistoryList";
            PcHistoryList.Attributes.Add("runat", "server");
            PcHistoryList.Attributes.Add("reversed", "reversed");

            ButtonControls.CssClass = "controls";

            AutomaticButton.ID = "AutomaticButton";
            AutomaticButton.Text = "AUTOMATIC";

            ManualButton.ID = "ManualButton";
            ManualButton.Text = "MANUAL";

            StartButton.ID = "StartButton";
            StartButton.Text = "START";

            PlayAgainButton.ID = "PlayAgainButton";
            PlayAgainButton.Text = "PLAY AGAIN";

            ManualShipsGenerator.ID = "ManualShipsGenerator";
            ManualShipsGenerator.Value = "MyValue";
            ManualShipsGenerator.EnableViewState = true;
        }

    }
}