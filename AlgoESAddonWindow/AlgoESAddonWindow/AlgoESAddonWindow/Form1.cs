using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Dynamic;
using System.Linq;
using System.Management.Instrumentation;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AlgoESAddonWindow.AlgoES
{
    public partial class formAlgoES : Form
    {

        private bool _boolStartTick = false;
        private bool _boolStartTrading = false;

        public formAlgoES()
        {
            InitializeComponent();

            panelLongTrade.AutoScroll = true;
            panelLongTrade.AutoSize = false;

            panelShortTrade.AutoScroll = true;
            panelShortTrade.AutoSize = false;
        }



        private void btnStartTrading_Click(object sender, EventArgs e)
        {

            if (_boolStartTrading == true)
            {
                btnStartTrading.Text = "Stop Trading";
                btnStartTrading.BackColor = Color.Gray;
                btnStartTrading.ForeColor = Color.Black;
            }
            else
            {
                btnStartTrading.Text = "Start Trading";
                btnStartTrading.BackColor = Color.Maroon;
                btnStartTrading.ForeColor = Color.White;
            }

            _boolStartTrading = !_boolStartTrading;
        }



        private void btnStartTick_Click(object sender, EventArgs e)
        {

            if (_boolStartTick == true)
            {

                btnStartTick.Text = "Stop Tick";
                btnStartTick.BackColor = Color.Gray;
                btnStartTick.ForeColor = Color.Black;

            }
            else
            {
                btnStartTick.Text = "Start Tick";
                btnStartTick.BackColor = Color.Maroon;
                btnStartTick.ForeColor = Color.White;
            }
            _boolStartTick = !_boolStartTick;

        }

        public void UpdateESUPrice(double price)
        {
            textESU24Tick.Text = price.ToString("F2");
        }

        public void UpdateESZPrice(double price)
        {
            textESZ24Tick.Text = price.ToString("F2");
        }

        public void UpdateESUOrderID(string orderID)
        {
            textESU24OrderID.Text = orderID;
        }

        public void UpdateESZOrderID(string orderID)
        {
            textESZ24OrderID.Text = orderID;
        }

        private void labelESZ24OrderID_Click(object sender, EventArgs e)
        {

        }

        private void btnSetLongBoxParams_Click(object sender, EventArgs e)
        {
        }

        private void ClearLongTradePanel()
        {
            panelLongTrade.Controls.Clear();
        }

        private void ClearShortTradePanel()
        {
            panelShortTrade.Controls.Clear();
        }

        private void btnSetShortBoxParams_Click(object sender, EventArgs e)
        {
        }

        private void btnSavePositionFile_Click(object sender, EventArgs e)
        {

        }

        private void btnLoadPositionFile_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

    }
}
