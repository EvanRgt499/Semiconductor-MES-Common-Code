-- 按工单+工序统计不良
SELECT
    sf.work_order_no,
    sf.op_code,
    sf.fail_code,
    fc.fail_name,
    COUNT(*) AS fail_cnt
FROM mes_sn_fail sf
LEFT JOIN mes_fail_code fc
    ON sf.fail_code = fc.fail_code
WHERE sf.work_order_no = :work_order_no
GROUP BY sf.work_order_no, sf.op_code, sf.fail_code, fc.fail_name
ORDER BY fail_cnt DESC;

-- 按日期+产线不良率统计
SELECT
    TRUNC(sf.create_time) AS stat_date,
    sf.line_code,
    COUNT(*) AS total_fail,
    ROUND(COUNT(*) / (SELECT COUNT(*) FROM mes_work_order_sn wos
                      WHERE TRUNC(wos.create_time) = TRUNC(sf.create_time)
                        AND wos.line_code = sf.line_code) * 100, 2) AS fail_rate
FROM mes_sn_fail sf
WHERE TRUNC(sf.create_time) BETWEEN :start_date AND :end_date
GROUP BY TRUNC(sf.create_time), sf.line_code
ORDER BY stat_date, line_code;