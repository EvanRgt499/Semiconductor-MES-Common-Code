-- ===============================================
-- 工单管理相关SQL脚本
-- 功能：工单基本信息查询、当日运行工单查询、工单状态重置
-- 优化：添加注释、参数化查询、性能优化
-- ===============================================

-- ------------------------------------------------
-- 工单基本信息查询
-- 功能：根据工单号查询工单详细信息
-- 参数：:work_order_no - 工单号
-- 性能优化：建议在work_order_no字段上创建索引
-- ------------------------------------------------
SELECT
    wo.work_order_no,         -- 工单号
    wo.part_no,               -- 物料号
    wo.qty_plan,              -- 计划数量
    wo.qty_complete,          -- 已完成数量
    wo.status,                -- 工单状态
    wo.line_code,             -- 产线代码
    wo.start_time,            -- 开始时间
    wo.create_time            -- 创建时间
FROM mes_work_order wo
WHERE wo.work_order_no = :work_order_no
ORDER BY wo.create_time DESC;  -- 按创建时间降序排列

-- ------------------------------------------------
-- 按产线查询当日运行工单
-- 功能：查询指定产线当天处于运行状态的工单
-- 参数：:line_code - 产线代码
-- 性能优化：建议在(line_code, status, create_time)上创建复合索引
-- ------------------------------------------------
SELECT
    work_order_no,            -- 工单号
    part_no,                  -- 物料号
    qty_plan,                 -- 计划数量
    qty_complete,             -- 已完成数量
    status                    -- 工单状态
FROM mes_work_order
WHERE line_code = :line_code
  AND TRUNC(create_time) = TRUNC(SYSDATE)  -- 当天数据
  AND status IN ('RELEASE','RUN')          -- 只查询释放和运行状态的工单
ORDER BY create_time DESC;                 -- 按创建时间降序排列

-- ------------------------------------------------
-- 重置工单状态（现场常用修复）
-- 功能：将工单状态重置为RELEASE状态
-- 参数：
--   :work_order_no - 工单号
--   :user - 操作人
-- 性能优化：建议在work_order_no字段上创建索引
-- ------------------------------------------------
UPDATE mes_work_order
SET
    status = 'RELEASE',       -- 重置为释放状态
    update_time = SYSDATE,    -- 更新时间为当前系统时间
    update_user = :user       -- 操作人
WHERE work_order_no = :work_order_no;

-- 提交事务
COMMIT;