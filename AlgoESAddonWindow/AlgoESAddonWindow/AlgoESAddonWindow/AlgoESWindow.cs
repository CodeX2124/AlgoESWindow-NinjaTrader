using AlgoESAddonWindow.AlgoES;
using AlgoESAddonWindow.OrderStrategy;
using AlgoESAddonWindow.SharedData;
using NinjaTrader.Cbi;
using NinjaTrader.Data;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Tools;
using NinjaTrader.NinjaScript;
using System.Windows;
using System.Windows.Threading;
using System.Xml.Linq;

namespace AlgoESAddonWindow
{
    public class MyWindowAddOn : AddOnBase
    {
        private formAlgoES algoWindow;
        private Instrument instrumentESU, instrumentESZ;
        private int cntESU = 1, cntESZ = 1;
        private double firstPriceESU, spreadESU, currentPriceESU, ESUlimitPrice;
        private double firstPriceESZ, spreadESZ, currentPriceESZ, ESZlimitPrice;
        private int ESUcontract, ESZcontract;
        private bool tickState = false;
        private NTMenuItem addonWindowMenuItem;
        private NTMenuItem existingMenuItemInControlCenter;
        private OrderPlacingStrategy orderStrategy;
        private string _OrderFilledInfo = "";
        private string orderIDESU, orderIDESZ;
        private double cashValue;
        private Account acc;
        private double cutLossValue;
        private bool cutLossActive = false;
        protected override void OnStateChange()
        {

            if (State == State.SetDefaults)
            {
                Description = "Example AddOnWindow of NinjaTrader 8";
                Name = "AddOnWindow";
            }

            if (State == State.DataLoaded)
            {
                acc = Account.All.FirstOrDefault(a => a.Name == "DEMO3279091");
            }

        }

        // Will be called as a new NTWindow is created. It will be called in the thread of that window
        protected override void OnWindowCreated(Window window)
        {

            // We want to place our AddOn in the Control Center's menus
            ControlCenter cc = window as ControlCenter;
            if (cc == null)
                return;

            /* Determine we want to place our AddOn in the Control Center's "New" menu
            Other menus can be accessed via the control's "Automation ID". For example: toolsMenuItem, workspacesMenuItem, connectionsMenuItem, helpMenuItem. */
            existingMenuItemInControlCenter = cc.FindFirst("ControlCenterMenuItemNew") as NTMenuItem;
            if (existingMenuItemInControlCenter == null)
                return;

            // 'Header' sets the name of our AddOn seen in the menu structure
            addonWindowMenuItem = new NTMenuItem
            {
                Header = "AddonWindow",
                Style = System.Windows.Application.Current.TryFindResource("MainMenuItem") as Style
            };

            // Add our AddOn into the "New" menu
            existingMenuItemInControlCenter.Items.Add(addonWindowMenuItem);

            // Subscribe to the event for when the user presses our AddOn's menu item
            addonWindowMenuItem.Click += OnMenuItemClick;

        }

        // Will be called as a new NTWindow is destroyed. It will be called in the thread of that window
        protected override void OnWindowDestroyed(Window window)
        {
            if (addonWindowMenuItem != null && window is ControlCenter)
            {
                if (existingMenuItemInControlCenter != null && existingMenuItemInControlCenter.Items.Contains(addonWindowMenuItem))
                    existingMenuItemInControlCenter.Items.Remove(addonWindowMenuItem);

                addonWindowMenuItem.Click -= OnMenuItemClick;
                addonWindowMenuItem = null;
            }
        }

        // Open our AddOn's window when the menu item is clicked on
        private void OnMenuItemClick(object sender, EventArgs e)
        {
            //Dispaly the AlgoES form
            algoWindow = new formAlgoES();
            System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                instrumentESU = Instrument.GetInstrument("ES SEP24");
                instrumentESZ = Instrument.GetInstrument("ES DEC24");
                algoWindow.btnStartTick.Click += StartTickClick;
                algoWindow.buttonBuyESU24.Click += PrepareOrderBuyESU;
                algoWindow.buttonSellESU24.Click += PrepareOrderSellESU;
                algoWindow.buttonBuyESZ24.Click += PrepareOrderBuyESZ;
                algoWindow.buttonSellESZ24.Click += PrepareOrderSellESZ;
                orderStrategy = new OrderPlacingStrategy();
                algoWindow.buttonCancelESU24Order.Click += CancelOrderESU;
                algoWindow.buttonCancelESZ24Order.Click += CancelOrderESZ;
                algoWindow.btnExecutionUpdateESU.Click += BtnExecutionUpdateESU;
                algoWindow.btnExecutionUpdateESZ.Click += BtnExecutionUpdateESZ;
                algoWindow.btnCutLoss.Click += BtnCutLossClick;
                algoWindow.Show();
            }));
        }
        private void OnMarketData(object sender, MarketDataEventArgs e)
        {
            if (acc == null)
                return;

            // Get current account cash value
            double currentValue = acc.Get(AccountItem.CashValue, Currency.UsDollar);

            // Update UI label safely
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                algoWindow.txtCurrentAccountValue.Text = $"{currentValue}";
            });

            // If cut loss is active, check condition
            if (cutLossActive && currentValue <= cutLossValue)
            {
                Print($"Cut loss triggered! Account value ${currentValue} <= ${cutLossValue}");

                orderStrategy.CloseAllOrder();

                // Disable cut loss after triggering
                cutLossActive = false;
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    algoWindow.btnCutLoss.Text = "No Cut Loss";
                });
            }

            //orderStrategy = new OrderPlacingStrategy();
            //if (tickState == true)
            //{

            //    if (e.Instrument.FullName == "ES SEP24")
            //    {
            //        if (e.MarketDataType == MarketDataType.Last)
            //        {
            //            currentPriceESU = e.Price;

            //            cntESU++;
            //            if (cntESU == 2)
            //            {
            //                firstPriceESU = currentPriceESU;
            //                Print($"First price of ES SEP24: {firstPriceESU}");
            //                algoWindow.UpdateESUPrice(firstPriceESU);
            //            }
            //            else
            //            {
            //                spreadESU = Math.Abs(currentPriceESU - firstPriceESU);
            //                if (spreadESU >= 0.25)
            //                {
            //                    algoWindow.UpdateESUPrice(currentPriceESU);
            //                    _OrderFilledInfo = orderStrategy.GetOrderFilledInfo();
            //                    //algoWindow.UpdateESUOrderID(_OrderFilledInfo);
            //                    // Print($"Current price of ES SEP24: {currentPriceESU}");
            //                    firstPriceESU = currentPriceESU;
            //                    spreadESU = 0;                                
            //                }
            //            }

            //        }
            //    }

            //    if (e.Instrument.FullName == "ES DEC24")
            //    {

            //        if (e.MarketDataType == MarketDataType.Last)
            //        {
            //            currentPriceESZ = e.Price;

            //            cntESZ++;
            //            if (cntESZ == 2)
            //            {
            //                firstPriceESZ = currentPriceESZ;
            //                Print($"First price of ES DEC24: {firstPriceESZ}");
            //                algoWindow.UpdateESZPrice(firstPriceESZ);
            //            }
            //            else
            //            {
            //                spreadESZ = Math.Abs(currentPriceESZ - firstPriceESZ);
            //                if (spreadESZ >= 0.25)
            //                {
            //                    algoWindow.UpdateESZPrice(currentPriceESZ);
            //                    //algoWindow.UpdateESZOrderID(_OrderFilledInfo);
            //                    //  Print($"Current price of ES DEC24: {currentPriceESZ}");
            //                    firstPriceESZ = currentPriceESZ;
            //                    spreadESZ = 0;
            //                }
            //            }

            //        }
            //    }
            //}

        }

        private void StartTickClick(object sender, EventArgs e)
        {

            System.Windows.Application.Current.Dispatcher.Invoke(() => {
                if (algoWindow.btnStartTick.Text == "Start Tick")
                {
                    tickState = true;
                    instrumentESU.MarketDataUpdate += OnMarketData;
                    instrumentESZ.MarketDataUpdate += OnMarketData;

                }

                else
                {
                    tickState = false;
                    instrumentESU.MarketDataUpdate -= OnMarketData;
                    instrumentESZ.MarketDataUpdate -= OnMarketData;
                }
            });


        }

        private void BtnCutLossClick(object sender, EventArgs e)
        {

            if (!cutLossActive)
            {
                // Try parse cut loss value from input
                if (double.TryParse(algoWindow.txtCutLossValue.Text, out double parsedValue) && parsedValue > 0)
                {
                    cutLossValue = parsedValue;
                    cutLossActive = true;
                    algoWindow.btnCutLoss.Text = "Set Cut Loss";
                    Print($"Cut loss set at {cutLossValue}");
                }
                else
                {
                    System.Windows.MessageBox.Show("Please enter a valid positive number for cut loss value.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            else
            {
                cutLossActive = false;
                algoWindow.btnCutLoss.Text = "No Cut Loss";
                Print("Cut loss disabled.");
            }
        }

        private void PrepareOrderBuyESU(object sender, EventArgs e)
        {

            if (tickState == true)
            {
                if (double.TryParse(algoWindow.textESU24LimitPrice.Text, out ESUlimitPrice) &&
                int.TryParse(algoWindow.textESU24NumContracts.Text, out ESUcontract))
                {
                    SharedDataESU.ContractCount = ESUcontract;
                    SharedDataESU.LimitPrice = ESUlimitPrice;
                    SharedDataESU.state = "buy";
                    SharedDataESU.instrument = instrumentESU;

                    orderStrategy = new OrderPlacingStrategy();
                    orderStrategy.PlaceOrderESU(); 
                    algoWindow.textESU24PlaceOrder.Text = $"orderID: {SharedDataESU.orderID}" + $" orderType: {SharedDataESU.state}" + $" LimitPrice: {SharedDataESU.LimitPrice}" + $" ContractCount: {SharedDataESU.ContractCount}";
                    algoWindow.textESU24LimitPrice.Text = "";
                    algoWindow.textESU24NumContracts.Text = "";
                }
                else
                {
                    System.Windows.MessageBox.Show("Invalid input for ESU24 order.");

                }
            }

        }

        private void PrepareOrderBuyESZ(object sender, EventArgs e)
        {
            if (tickState == true)
            {
                if (double.TryParse(algoWindow.textESZ24LimitPrice.Text, out ESZlimitPrice) &&
                    int.TryParse(algoWindow.textESZ24NumContracts.Text, out ESZcontract))
                {
                    SharedDataESZ.ContractCount = ESZcontract;
                    SharedDataESZ.LimitPrice = ESZlimitPrice;
                    SharedDataESZ.state = "buy";
                    SharedDataESZ.instrument = instrumentESZ;

                    orderStrategy = new OrderPlacingStrategy();
                    orderStrategy.PlaceOrderESZ();
                    algoWindow.textESZ24PlaceOrder.Text = $"orderID: {SharedDataESZ.orderID}" + $" orderType: {SharedDataESZ.state}" + $" LimitPrice: {SharedDataESZ.LimitPrice}" + $" ContractCount: {SharedDataESZ.ContractCount}";
                    algoWindow.textESZ24LimitPrice.Text = "";
                    algoWindow.textESZ24NumContracts.Text = "";
                }
                //else
                //{
                //    System.Windows.MessageBox.Show("Invalid input for ESZ24 order.");
                //}
            }

        }

        private void PrepareOrderSellESU(object sender, EventArgs e)
        {
            if (tickState == true)
            {
                if (double.TryParse(algoWindow.textESU24LimitPrice.Text, out ESUlimitPrice) &&
                    int.TryParse(algoWindow.textESU24NumContracts.Text, out ESUcontract))
                {
                    SharedDataESU.LimitPrice = ESUlimitPrice;
                    SharedDataESU.ContractCount = ESUcontract;
                    SharedDataESU.state = "sell";
                    SharedDataESU.instrument = instrumentESU;

                    orderStrategy = new OrderPlacingStrategy();
                    orderStrategy.PlaceOrderESU();
                    algoWindow.textESU24PlaceOrder.Text = $"orderID: {SharedDataESU.orderID}" + $" orderType: {SharedDataESU.state}" + $" LimitPrice: {SharedDataESU.LimitPrice}" + $" ContractCount: {SharedDataESU.ContractCount}";
                    algoWindow.textESU24LimitPrice.Text = "";
                    algoWindow.textESU24NumContracts.Text = "";
                }
                //else
                //{
                //    System.Windows.MessageBox.Show("Invalid input for ESU24 order.");
                //}
            }

        }

        private void PrepareOrderSellESZ(object sender, EventArgs e)
        {
            if (tickState == true)
            {
                if (double.TryParse(algoWindow.textESZ24LimitPrice.Text, out ESZlimitPrice) &&
                    int.TryParse(algoWindow.textESZ24NumContracts.Text, out ESZcontract))
                {
                    SharedDataESZ.LimitPrice = ESZlimitPrice;
                    SharedDataESZ.ContractCount = ESZcontract;
                    SharedDataESZ.state = "sell";
                    SharedDataESZ.instrument = instrumentESZ;

                    orderStrategy = new OrderPlacingStrategy();
                    orderStrategy.PlaceOrderESZ();
                    algoWindow.textESZ24PlaceOrder.Text = $"orderID: {SharedDataESZ.orderID}" + $" orderType: {SharedDataESZ.state}" + $" LimitPrice: {SharedDataESU.LimitPrice}" + $" ContractCount: {SharedDataESZ.ContractCount}";
                    algoWindow.textESZ24LimitPrice.Text = "";
                    algoWindow.textESZ24NumContracts.Text = "";
                }
                else
                {
                    System.Windows.MessageBox.Show("Invalid input for ESZ24 order.");
                }
            }
        }

        private void CancelOrderESU(object sender, EventArgs e)
        {
            algoWindow.textESU24PlaceOrder.Text = "";
            orderStrategy.CancelOrderByOrderID(algoWindow.textESU24OrderID.Text);
        }

        private void CancelOrderESZ(object sender, EventArgs e)
        {
            algoWindow.textESZ24PlaceOrder.Text = "";
            orderStrategy.CancelOrderByOrderID(algoWindow.textESZ24OrderID.Text);
        }

        private void BtnExecutionUpdateESU(object sender, EventArgs e)
        {
            orderIDESU = orderStrategy.GetExecution(algoWindow.textESU24OrderID.Text);
            algoWindow.textESU24PlaceOrder.Text = orderIDESU;
            
        }

        private void BtnExecutionUpdateESZ(object sender, EventArgs e)
        {
            orderIDESZ = orderStrategy.GetExecution(algoWindow.textESZ24OrderID.Text);
            algoWindow.textESZ24PlaceOrder.Text = orderIDESZ;
        }
    }

    /* Class which implements Tools.INTTabFactory must be created and set as an attached property for TabControl
    in order to use tab page add/remove/move/duplicate functionality */
    public class AddOnWindowFactory : INTTabFactory
    {
        // INTTabFactory member. Required to create parent window
        public NTWindow CreateParentWindow()
        {
            return new MyCustomAddOnWindow();
        }

        // INTTabFactory member. Required to create tabs
        public NTTabPage CreateTabPage(string typeName, bool isTrue)
        {
            return new MyCustomTabPage();
        }
    }

    public class MyCustomTabPage : NTTabPage
    {
        private formAlgoES algoForm;
        private System.Windows.Controls.TabControl MainTabControl;
        public MyCustomTabPage()
        {
            algoForm = new formAlgoES();
            this.Content = algoForm;
        }

        protected override string GetHeaderPart(string headerText)
        {
            return headerText;
        }

        // IWorkspacePersistence member. Required for restoring window from workspace
        protected override void Restore(XElement element)
        {
            if (MainTabControl != null)
                MainTabControl.RestoreFromXElement(element);
        }

        // IWorkspacePersistence member. Required for saving window to workspace
        protected override void Save(XElement element)
        {
            if (MainTabControl != null)
                MainTabControl.SaveToXElement(element);
        }

    }

    public class MyCustomAddOnWindow : NTWindow, IWorkspacePersistence
    {
        public MyCustomAddOnWindow()
        {
            // set Caption property (not Title), since Title is managed internally to properly combine selected Tab Header and Caption for display in the windows taskbar
            // This is the name displayed in the top-left of the window
            Caption = "AddOnWindow";

            // Set the default dimensions of the window
            Width = 1085;
            Height = 900;

            // TabControl should be created for window content if tab features are wanted
            System.Windows.Controls.TabControl tc = new System.Windows.Controls.TabControl();

            // Attached properties defined in TabControlManager class should be set to achieve tab moving, adding/removing tabs
            TabControlManager.SetIsMovable(tc, true);
            TabControlManager.SetCanAddTabs(tc, true);
            TabControlManager.SetCanRemoveTabs(tc, true);

            // if ability to add new tabs is desired, TabControl has to have attached property "Factory" set.
            TabControlManager.SetFactory(tc, new AddOnWindowFactory());
            Content = tc;

            /* In order to have link buttons functionality, tab control items must be derived from Tools.NTTabPage
            They can be added using extention method AddNTTabPage(NTTabPage page) */
            tc.AddNTTabPage(new MyCustomTabPage());

            // WorkspaceOptions property must be set
           // Loaded += (o, e) =>
          //  {
           //     if (WorkspaceOptions == null)
          //          WorkspaceOptions = new WorkspaceOptions("AddOnWindow-" + Guid.NewGuid().ToString("N"), this);
           // };
        }

        // IWorkspacePersistence member. Required for restoring window from workspace
        public void Restore(XDocument document, XElement element)
        {
            if (MainTabControl != null)
                MainTabControl.RestoreFromXElement(element);
        }

        // IWorkspacePersistence member. Required for saving window to workspace
        public void Save(XDocument document, XElement element)
        {
            if (MainTabControl != null)
                MainTabControl.SaveToXElement(element);
        }

        // IWorkspacePersistence member
        public WorkspaceOptions WorkspaceOptions
        { get; set; }
    }
}
