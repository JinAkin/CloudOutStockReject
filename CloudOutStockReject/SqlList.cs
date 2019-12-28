namespace CloudOutStockReject
{
    public class SqlList
    {
        private string _result;

        /// <summary>
        /// 以销售出库单的单据为条件,更新其对应的销售订单-‘剩余未出库金额’
        /// 执行顺序:1)先根据获取的销售出库单单据查找出对应的销售订单,然后再更新
        /// </summary>
        /// <param name="orderno"></param>
        /// <returns></returns>
        public string GetUpdate(string orderno)
        {
            _result = $@"
                            BEGIN
	                                --收集‘销售订单’的相关记录
                                    IF OBJECT_ID('tempdb..#temp0') IS NOT NULL
		                                DROP TABLE #temp0
	                                IF OBJECT_ID('tempdb..#temp') IS NOT NULL
		                                DROP TABLE #temp
	                                IF OBJECT_ID('tempdb..#temp1') IS NOT NULL
		                                DROP TABLE #temp1
	                                IF OBJECT_ID('tempdb..#temp2') IS NOT NULL
		                                DROP TABLE #temp2

	                                --根据销售出库单找到对应的销售订单FENTRYID
	                                SELECT DISTINCT e.FENTRYID
	                                INTO #temp0
	                                FROM dbo.T_SAL_OUTSTOCK A
	                                INNER JOIN dbo.T_SAL_OUTSTOCKENTRY b ON a.FID=b.FID
	                                INNER JOIN dbo.T_SAL_OUTSTOCKENTRY_LK c ON b.FENTRYID=c.FENTRYID
	                                INNER JOIN T_SAL_DELIVERYNOTICEENTRY_LK d ON c.FSID=d.FENTRYID
	                                INNER JOIN T_SAL_ORDERENTRY e ON d.FSID=e.FENTRYID
	                                WHERE A.FBILLNO='{orderno}'

	                                SELECT B.FENTRYID '销售订单FENTRYID',c.FALLAMOUNT '销售订单-价税合计',d.FENTRYID '发货通知单FENTRYID'
	                                INTO #TEMP
	                                FROM dbo.T_SAL_ORDER A
	                                INNER JOIN dbo.T_SAL_ORDERENTRY B ON A.FID=B.FID
	                                INNER JOIN dbo.T_SAL_ORDERENTRY_F c ON b.FENTRYID=c.FENTRYID
	                                INNER JOIN dbo.T_SAL_DELIVERYNOTICEENTRY_LK d ON b.FENTRYID=d.FSID
	                                WHERE /*CONVERT(varchar(100),a.FDATE, 23)>='2019-11-01' 
	                                AND CONVERT(varchar(100),a.FDATE, 23)<='2019-12-31'*/
	                                EXISTS (
				                                SELECT null FROM #temp0 t
				                                WHERE b.FENTRYID=t.FENTRYID
			                                )
	                                ORDER BY B.FENTRYID

	                                --根据‘发货通知单FENTRYID’找出对应的‘销售出库单FENTRYID’记录，若有就执行公式：剩余未出库金额=销售订单价税合计-销售出库单价税合计
	                                --若无就执行公式:剩余未出库金额=销售订单价税合计-0
	                                --找出有‘发货通知单’的相关记录
	                                SELECT a.销售订单FENTRYID,a.[销售订单-价税合计],a.发货通知单FENTRYID
	                                INTO #temp1
	                                FROM #TEMP A
	                                WHERE EXISTS (
					                                SELECT NULL
					                                FROM dbo.T_SAL_OUTSTOCKENTRY_LK A1
					                                WHERE A.发货通知单FENTRYID=A1.FSID
				                                )

	                                --找到没有‘发货通知单’的相关记录
	                                SELECT a.销售订单FENTRYID,a.[销售订单-价税合计]
	                                INTO #temp2
	                                FROM #TEMP A
	                                WHERE NOT EXISTS (
					                                SELECT NULL
					                                FROM dbo.T_SAL_OUTSTOCKENTRY_LK A1
					                                WHERE A.发货通知单FENTRYID=A1.FSID
				                                )

	                                --更新
	                                --先更新存在销售出库单的记录
                                    UPDATE A SET A.F_YTC_DECIMAL5=X.[销售订单-价税合计]-X.[销售出库单-价税合计]
                                    FROM dbo.T_SAL_ORDERENTRY A
	                                INNER JOIN (
					                                SELECT A1.销售订单FENTRYID,A1.[销售订单-价税合计],SUM(A4.FALLAMOUNT) '销售出库单-价税合计'
					                                FROM #temp1 A1
					                                INNER JOIN dbo.T_SAL_OUTSTOCKENTRY_LK A2 ON A1.发货通知单FENTRYID=A2.FSID
					                                INNER JOIN dbo.T_SAL_OUTSTOCKENTRY A3 ON A2.FENTRYID=A3.FENTRYID
					                                INNER JOIN dbo.T_SAL_OUTSTOCKENTRY_F A4 ON A3.FENTRYID=A4.FENTRYID
					                                GROUP BY a1.销售订单FENTRYID,a1.[销售订单-价税合计]
				                                ) AS X ON  X.销售订单FENTRYID=A.FENTRYID

	                                --再更新不存在销售出库单的记录
	                                UPDATE B SET B.F_YTC_DECIMAL5=B1.[销售订单-价税合计]-0
	                                FROM dbo.T_SAL_ORDERENTRY B 
	                                INNER JOIN #temp2 B1 ON B.FENTRYID=B1.销售订单FENTRYID

	                                SELECT 'FINISH'
                                END
                        ";
            return _result;
        }

    }
}
