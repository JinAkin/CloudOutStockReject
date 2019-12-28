using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;

namespace CloudOutStockReject
{
    public class ButtonEvents : AbstractBillPlugIn
    {
        Generate generate=new Generate();

        public override void BarItemClick(BarItemClickEventArgs e)
        {
            //订单退回操作
            base.BarItemClick(e);

            //销售出库单-反审核时执行
            if (e.BarItemKey == "tbReject")
            {
                //定义获取表头信息对像
                var docScddIds1 = View.Model.DataObject;
                //获取表头中单据编号信息(注:这里的BillNo为单据编号中"绑定实体属性"项中获得)
                var dhstr = docScddIds1["BillNo"].ToString();

                //根据所获取的‘销售出库单’单号执行相关的SQL
                generate.UpdateK3Record(dhstr);
            }
        }
    }
}
