-- ===============================================
-- 质量与不良统计相关SQL脚本
-- 功能：工单工序不良统计、日期产线不良率统计
-- 优化：添加注释、参数化查询、性能优化
-- ===============================================

-- ------------------------------------------------
-- 按工单+工序统计不良
-- 功能：统计指定工单各工序的不良情况
-- 参数：:work_order_no - 工单号
-- 性能优化：建议在(work_order_no, op_code, fail_code)上创建复合索引
-- ------------------------------------------------
SELECT
    sf.work_order_no,    -- 工单号
    sf.op_code,          -- 工序代码
    sf.fail_code,        -- 不良代码
    fc.fail_name,        -- 不良名称
    COUNT(*) AS fail_cnt -- 不良数量
FROM mes_sn_fail sf
LEFT JOIN mes_fail_code fc
    ON sf.fail_code = fc.fail_code  -- 关联不良代码表获取不良名称
WHERE sf.work_order_no = :work_order_no
GROUP BY sf.work_order_no, sf.op_code, sf.fail_code, fc.fail_name
ORDER BY fail_cnt DESC;  -- 按不良数量降序排列

-- ------------------------------------------------
-- 按日期+产线不良率统计
-- 功能：统计指定时间范围内各产线的不良率
-- 参数：
--   :start_date - 开始日期
--   :end_date - 结束日期
-- 性能优化：
--   1. 建议在(create_time, line_code)上创建复合索引
--   2. 考虑使用分析函数提高性能
-- ------------------------------------------------
SELECT
    TRUNC(sf.create_time) AS stat_date,  -- 统计日期
    sf.line_code,                      -- 产线代码
    COUNT(*) AS total_fail,            -- 不良总数
    -- 计算不良率：不良数 / 总过站数 * 100，保留2位小数
    ROUND(
        COUNT(*) / 
        (SELECT COUNT(*) 
         FROM mes_work_order_sn wos
         WHERE TRUNC(wos.create_time) = TRUNC(sf.create_time)
           AND wos.line_code = sf.line_code
        ) * 100, 
        2
    ) AS fail_rate  -- 不良率（%）
FROM mes_sn_fail sf
WHERE TRUNC(sf.create_time) BETWEEN :start_date AND :end_date  -- 指定时间范围
GROUP BY TRUNC(sf.create_time), sf.line_code  -- 按日期和产线分组
ORDER BY stat_date, line_code;  -- 按日期和产线排序