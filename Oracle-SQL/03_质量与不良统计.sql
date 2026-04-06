-- 半导体封装MES系统 - 质量与不良统计SQL脚本
-- 作者：[孟斯辰]
-- 日期：2026-04-06
-- 说明：用于质量分析和不良统计

-- ============================
-- 1. 工单工序不良统计
-- 用途：分析指定工单各工序的不良分布情况
-- 场景：质量分析会议、工单评审
-- 注意：不良代码格式为：工序代码+序号（如：DB01/WB02等）
-- ============================
SELECT
    sf.work_order_no,    -- 工单号
    sf.op_code,          -- 工序代码
    sf.fail_code,        -- 不良代码
    fc.fail_name,        -- 不良名称
    COUNT(*) AS fail_cnt -- 不良数量
FROM mes_sn_fail sf
LEFT JOIN mes_fail_code fc
    ON sf.fail_code = fc.fail_code  -- 关联不良代码表
WHERE sf.work_order_no = :work_order_no  -- 工单号参数
GROUP BY sf.work_order_no, sf.op_code, sf.fail_code, fc.fail_name
ORDER BY fail_cnt DESC;  -- 按不良数量降序排列

-- ============================
-- 2. 产线不良率日报
-- 用途：统计各产线每日不良率
-- 场景：生产早会、质量报告
-- 计算方式：不良数 / 总过站数 * 100%
-- ============================
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
WHERE TRUNC(sf.create_time) BETWEEN :start_date AND :end_date  -- 时间范围
GROUP BY TRUNC(sf.create_time), sf.line_code  -- 按日期和产线分组
ORDER BY stat_date, line_code;  -- 按日期和产线排序

-- 历史记录：
-- 2026-04-06：初始版本
-- 后续计划添加：不良趋势分析、不良分类统计