using NinjaTrader.Cbi;
using NinjaTrader.NinjaScript;
using AlgoESAddonWindow.SharedData;
using NinjaTrader.Data;
using AlgoESAddonWindow.AlgoES;
using System.Windows.Controls;
using System.Windows;

//This namespace holds Strategies in this folder and is required. Do not change it. 

namespace AlgoESAddonWindow.OrderStrategy
{

    public class OrderPlacingStrategy : StrategyBase
    {

        private string orderplacingID = "";
        public string[] orderIDs = new string[100];
        public double[] Prices = new double[100];
        public double[] limitPrices = new double[100];
        public string[] states = new string[100];
        public MarketPosition[] marketPositions = new MarketPosition[100];
        public int[] quantities = new int[100];
        private Order[] OrdersESU = new Order[100];
        private Order[] OrdersESZ = new Order[100];
        private Order realOrderESU;
        private Order realOrderESZ;
        private double limitPriceESU, limitPriceESZ, currentPriceESU, currentPriceESZ;
        private int contractCountESU = 0, contractCountESZ = 0, cnt = 0, cntESU = 0, cntESZ = 0, cntFilled = 0;
        public string typeESU = "", typeESZ = "", currentOrderId;
        private formAlgoES algoForm;
        private Instrument instrumentESU, instrumentESZ;
        private string _strOrderFilledInfo = "", orderState = "None";
        private void OnMarketData(object sender, MarketDataEventArgs e)
        {

            if (e.Instrument.FullName == "ES SEP24")
            {
                if (e.MarketDataType == MarketDataType.Last)
                {
                    currentPriceESU = e.Price;
                    for (int i = 0; i < SharedDataState.cnt; i++)
                    {
                        if (Math.Abs(currentPriceESU - SharedDataState.limitPrices[i]) >= 2)
                        {
                            Account acc = Account.All.FirstOrDefault(a => a.Name == "DEMO2877218");
                            CancelOrder(acc, SharedDataState.orderIDs[i]);
                            // Print($"{i} ====> {currentOrders[i].OrderId} cancelled due to high/low spread");
                        }
                    }

                }
            }

            if (e.Instrument.FullName == "ES DEC24")
            {
                if (e.MarketDataType == MarketDataType.Last)
                {
                    currentPriceESZ = e.Price;
                    for (int i = 0; i < SharedDataState.cnt; i++)
                    {
                        if (Math.Abs(currentPriceESZ - SharedDataState.limitPrices[i]) >= 2)
                        {
                            Account acc = Account.All.FirstOrDefault(a => a.Name == "DEMO2877218");
                            CancelOrder(acc, SharedDataState.orderIDs[i]);
                            // Print($"{i} ====> {currentOrders[i].OrderId} cancelled due to high/low spread");
                        }
                    }
                }
            }


        }

        public string GetOrderFilledInfo()
        {
            //OrderFilledInfo = _strOrderFilledInfo;
            return _strOrderFilledInfo;
        }

        public string GetExecution(string id)
        {
            
            for (int i = 0; i < SharedDataState.cnt; i++)
            {
                
                if(id == SharedDataState.orderIDs[i])
                {
                    orderState = SharedDataState.states[i];
                }
            }
            
            return orderState;
        }

        public void OnOrderUpdate(object sender, OrderEventArgs e)
        {
            if (e.OrderState == OrderState.Filled)
            {
                _strOrderFilledInfo = $"Order : {e.OrderId}" + "filled at price" + $"{e.AverageFillPrice} successfully";
                GetOrderFilledInfo();
                Print($"Order : {e.OrderId} filled at price {e.AverageFillPrice} successfully");
                for (int i = 0; i < SharedDataState.cnt; i++)
                {
                    if (e.OrderId == SharedDataState.orderIDs[i])
                        SharedDataState.states[i] = "Filled";
                }
            }

            if (e.OrderState == OrderState.Cancelled)
            {
                Print($"Order {e.OrderId} cancelled.");
                for (int i = 0; i < SharedDataState.cnt; i++) 
                {
                    if (e.OrderId == SharedDataState.orderIDs[i])
                        SharedDataState.states[i] = "Cancelled";
                }
            }

            if (e.OrderState == OrderState.Working)
            {
                if (SharedDataState.cnt == 0)
                {
                    SharedDataState.orderIDs[SharedDataState.cnt] = e.OrderId;
                    SharedDataState.limitPrices[SharedDataState.cnt] = e.LimitPrice;
                    SharedDataState.states[SharedDataState.cnt] = "Working";
                    SharedDataState.cnt++;
                }
                else if (SharedDataState.orderIDs[SharedDataState.cnt -1] != e.OrderId)
                {
                    SharedDataState.orderIDs[SharedDataState.cnt] = e.OrderId;
                    SharedDataState.limitPrices[SharedDataState.cnt] = e.LimitPrice;
                    SharedDataState.states[SharedDataState.cnt] = "Working";
                    SharedDataState.cnt++;
                }
                
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    instrumentESU = Instrument.GetInstrument("ES SEP24");
                    instrumentESZ = Instrument.GetInstrument("ES DEC24");
                    instrumentESU.MarketDataUpdate += OnMarketData;
                    instrumentESZ.MarketDataUpdate += OnMarketData;
                });


            }
        }

        // Define the SendOrder function
        public NinjaTrader.Cbi.Order SendOrder(Account account, Instrument instrument, OrderAction orderAction, OrderType orderType, int qty, double limitPrice, double stopPrice, string nameSignal)
        {
            if (account == null || instrument == null)
            {
                Print("Account or Instrument is null.");
                return null;
            }

            NinjaTrader.Cbi.Order order = account.CreateOrder(instrument, orderAction, orderType, TimeInForce.Gtc, qty, limitPrice, stopPrice, string.Empty, nameSignal, null);

            account.Submit(new[] { order });

            return order;
        }
        public void PlaceOrderESU()
        {
            // Read data from shared class
            limitPriceESU = SharedDataESU.LimitPrice;
            contractCountESU = SharedDataESU.ContractCount;
            typeESU = SharedDataESU.state;
            instrumentESU = SharedDataESU.instrument;
            algoForm = new formAlgoES();

            if (typeESU == "buy" && limitPriceESU > 0 && contractCountESU > 0)
            {
                try
                {
                    // Gather required parameters for SendOrder function
                    Account account = Account.All.FirstOrDefault(a => a.Name == "DEMO2877218"); // Get the first available account (modify as per your requirement)

                    if (account == null)
                    {
                        Print("No account found.");
                        return;
                    }

                    if (instrumentESU == null)
                    {
                        Print("No instrument found for ESU.");
                        return;
                    }

                    // Call the SendOrder function
                    realOrderESU = SendOrder(account, instrumentESU, OrderAction.Buy, OrderType.Limit, contractCountESU, limitPriceESU, 0, "LimitBuyOrder");
                    SharedDataESU.orderID = realOrderESU.OrderId;
                    OrdersESU[cntESU] = new Order
                    {
                        LimitPrice = realOrderESU.LimitPrice,
                        OrderId = realOrderESU.OrderId
                    };
                    cntESU++;
                    Print($"BuyLimit order successfully placed.");
                    
                }
                catch (Exception err)
                {
                    Print($"Error placing BuyLimit order: {err.Message}");
                }

                // Reset the parameters in the shared class to prevent repeated ordering                
                limitPriceESU = 0;
                contractCountESU = 0;
            }
            else
            {
                Print("Order conditions not met for ESU.");
            }

            if (typeESU == "sell" && limitPriceESU > 0 && contractCountESU > 0)
            {
                try
                {
                    // Gather required parameters for SendOrder function
                    Account account = Account.All.FirstOrDefault(a => a.Name == "DEMO2877218"); // Get the first available account (modify as per your requirement)

                    if (account == null)
                    {
                        Print("No account found.");
                        return;
                    }

                    if (instrumentESU == null)
                    {
                        Print("No instrument found for ESU.");
                        return;
                    }

                    // Call the SendOrder function
                    realOrderESU = SendOrder(account, instrumentESU, OrderAction.Sell, OrderType.Limit, contractCountESU, limitPriceESU, 0, "LimitSellOrder");
                    SharedDataESU.orderID = realOrderESU.OrderId;
                    OrdersESU[cntESU] = new Order
                    {
                        LimitPrice = realOrderESU.LimitPrice,
                        OrderId = realOrderESU.OrderId
                    };
                    cntESU++;
                    Print("SellLimit order successfully placed.");
                }
                catch (Exception err)
                {
                    Print($"Error placing SellLimit order: {err.Message}");
                }

                // Reset the parameters in the shared class to prevent repeated ordering                
                limitPriceESU = 0;
                contractCountESU = 0;
            }
            else
            {
                Print("Order conditions not met for ESU.");
            }
        }

        public void PlaceOrderESZ()
        {
            // Read data from shared class
            limitPriceESZ = SharedDataESZ.LimitPrice;
            contractCountESZ = SharedDataESZ.ContractCount;
            typeESZ = SharedDataESZ.state;
            instrumentESZ = SharedDataESZ.instrument;
            algoForm = new formAlgoES();

            if (typeESZ == "buy" && limitPriceESZ > 0 && contractCountESZ > 0)
            {
                try
                {
                    // Gather required parameters for SendOrder function
                    Account account = Account.All.FirstOrDefault(a => a.Name == "DEMO2877218"); // Get the first available account (modify as per your requirement)

                    if (account == null)
                    {
                        Print("No account found.");
                        return;
                    }

                    if (instrumentESZ == null)
                    {
                        Print("No instrument found for ESZ.");
                        return;
                    }

                    // Call the SendOrder function
                    realOrderESZ = SendOrder(account, instrumentESZ, OrderAction.Buy, OrderType.Limit, contractCountESZ, limitPriceESZ, 0, "LimitBuyOrder");
                    SharedDataESZ.orderID = realOrderESZ.OrderId;
                    OrdersESZ[cntESZ] = new Order
                    {
                        LimitPrice = realOrderESZ.LimitPrice,
                        OrderId = realOrderESZ.OrderId
                    };
                    cntESZ++;
                    Print("BuyLimit order successfully placed.");
                }
                catch (Exception err)
                {
                    Print($"Error placing BuyLimit order: {err.Message}");
                }

                // Reset the parameters in the shared class to prevent repeated ordering                
                limitPriceESZ = 0;
                contractCountESZ = 0;
            }

            if (typeESZ == "sell" && limitPriceESZ > 0 && contractCountESZ > 0)
            {
                try
                {
                    // Gather required parameters for SendOrder function
                    Account account = Account.All.FirstOrDefault(a => a.Name == "DEMO2877218"); // Get the first available account (modify as per your requirement)

                    if (account == null)
                    {
                        Print("No account found.");
                        return;
                    }

                    if (instrumentESZ == null)
                    {
                        Print("No instrument found for ESZ.");
                        return;
                    }

                    // Call the SendOrder function
                    realOrderESZ = SendOrder(account, instrumentESZ, OrderAction.Sell, OrderType.Limit, contractCountESZ, limitPriceESZ, 0, "LimitSellOrder");
                    SharedDataESZ.orderID = realOrderESZ.OrderId;
                    OrdersESZ[cntESZ] = new Order
                    {
                        LimitPrice = realOrderESZ.LimitPrice,
                        OrderId = realOrderESZ.OrderId
                    };
                    cntESZ++;
                    Print("SellLimit order successfully placed.");
                }
                catch (Exception err)
                {
                    Print($"Error placing SellLimit order: {err.Message}");
                }

                // Reset the parameters in the shared class to prevent repeated ordering                
                limitPriceESZ = 0;
                contractCountESZ = 0;
            }
        }

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = "Sample order placing strategy.";
                Name = "OrderPlacingStrategy";
                Calculate = Calculate.OnEachTick;
                Account account = Account.All.FirstOrDefault(a => a.Name == "DEMO2877218");
                account.OrderUpdate += OnOrderUpdate;
            }
            if (State == State.Configure)
            {
                AddDataSeries(NinjaTrader.Data.BarsPeriodType.Minute, 1);
                Print("State: Order Configure");
            }
        }

        public void CancelOrder(Account account, string OrderId)
        {
            List<Order> orders = account.Orders.Where(o => isWorkingState(o) && o.OrderId == OrderId).ToList();

            account.Cancel(orders);
        }

        
        private bool isWorkingState(Order order)
        {
            return order.OrderState == OrderState.Working;
        }


        public void CancelOrderByOrderID(string OrderId)
        {
            Account account = Account.All.FirstOrDefault(a => a.Name == "DEMO2877218");
            //Account account = Account.All.FirstOrDefault(a => a.Name == "DEMO2877218");

            CancelOrder(account, OrderId);

        }

        /* public void CanceltLimitOrderESU()
         {
             Account account = Account.All.FirstOrDefault(a => a.Name == "DEMO2877218");

             if (account != null && SharedDataESU.orderID != "")
             {
                 CancelOrder(account, SharedDataESU.orderID);
                 Print($"Cancel request sent for order ID: {SharedDataESU.orderID}");
             }
             else
             {
                 Print("No active order to cancel");
             }

         }

         public void CanceltLimitOrderESZ()
         {
             Account account = Account.All.FirstOrDefault(a => a.Name == "DEMO2877218");

             if (account != null && SharedDataESZ.orderID != "")
             {
                 CancelOrder(account, SharedDataESZ.orderID);
                 Print($"Cancel request sent for order ID: {SharedDataESZ.orderID}");
             }
             else
             {
                 Print("No active order to cancel");
             }
         }*/
    }
}
