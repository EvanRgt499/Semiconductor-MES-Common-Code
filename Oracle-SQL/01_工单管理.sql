-- 半导体封装MES系统 - 工单管理SQL脚本
-- 作者：[孟斯辰]
-- 日期：2026-04-06
-- 说明：包含工单查询和状态管理常用语句

-- ============================
-- 1. 工单基本信息查询
-- 用途：根据工单号查询完整工单信息
-- 注意：在work_order_no字段上已创建索引，查询速度较快
-- ============================
SELECT
    wo.work_order_no,     -- 工单号（格式：WO+年月日+流水号）
    wo.part_no,           -- 物料号（芯片型号）
    wo.qty_plan,          -- 计划数量
    wo.qty_complete,      -- 已完成数量
    wo.status,            -- 工单状态（RELEASE/RUN/COMPLETE）
    wo.line_code,         -- 产线代码（F1/F2/F3等）
    wo.start_time,        -- 开始时间
    wo.create_time        -- 创建时间
FROM mes_work_order wo
WHERE wo.work_order_no = :work_order_no  -- 工单号参数
ORDER BY wo.create_time DESC;  -- 最新创建的排在前面

-- ============================
-- 2. 当日产线运行工单查询
-- 用途：查看指定产线当天正在生产的工单
-- 场景：生产调度人员日常监控
-- ============================
SELECT
    work_order_no,        -- 工单号
    part_no,              -- 物料号
    qty_plan,             -- 计划数量
    qty_complete,         -- 已完成数量
    status                -- 工单状态
FROM mes_work_order
WHERE line_code = :line_code  -- 产线代码参数
  AND TRUNC(create_time) = TRUNC(SYSDATE)  -- 只查询当天数据
  AND status IN ('RELEASE','RUN')  -- 释放和运行状态
ORDER BY create_time DESC;  -- 按创建时间排序

-- ============================
-- 3. 工单状态重置（现场修复用）
-- 用途：当工单状态异常时，重置为初始状态
-- 注意：此操作会影响生产流程，谨慎使用
-- ============================
UPDATE mes_work_order
SET
    status = 'RELEASE',     -- 重置为释放状态
    update_time = SYSDATE,  -- 更新时间戳
    update_user = :user     -- 操作人
WHERE work_order_no = :work_order_no;  -- 目标工单

-- 提交事务
COMMIT;

-- 历史记录：
-- 2026-04-06：初始版本
-- 后续可根据实际需求添加更多查询语句