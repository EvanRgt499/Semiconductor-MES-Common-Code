-- 半导体封装MES系统 - 产品过站与追溯SQL脚本
-- 作者：[孟斯辰]
-- 日期：2026-04-06
-- 说明：用于产品生产过程追溯和状态查询

-- ============================
-- 1. SN完整过站履历查询
-- 用途：查询芯片从投料到完成的所有工序记录
-- 场景：质量追溯、问题分析
-- 注意：SN格式为：FAB+批次号+流水号
-- ============================
SELECT
    wos.sn,               -- 产品序列号
    wos.work_order_no,    -- 工单号
    wos.op_code,          -- 工序代码（如：DB/WB/FP等）
    op.op_name,           -- 工序名称
    wos.equip_code,       -- 设备代码（如：DB01/WB02等）
    wos.check_result,     -- 检查结果（PASS/FAIL）
    wos.create_time       -- 过站时间
FROM mes_work_order_sn wos
LEFT JOIN mes_operation op
    ON wos.op_code = op.op_code  -- 关联工序表获取名称
WHERE wos.sn = :sn  -- SN参数
ORDER BY wos.create_time ASC;  -- 按时间顺序排列

-- ============================
-- 2. 工单工序未过站SN查询
-- 用途：查找指定工单在某工序待处理的芯片
-- 场景：生产进度跟踪、工序 bottleneck 分析
-- ============================
SELECT
    sn,               -- 产品序列号
    work_order_no,    -- 工单号
    op_code,          -- 工序代码
    create_time       -- 创建时间
FROM mes_work_order_sn
WHERE work_order_no = :work_order_no  -- 工单号参数
  AND op_code = :op_code  -- 工序代码参数
  AND check_result IS NULL  -- 未完成过站
ORDER BY create_time ASC;  -- 按创建时间排序

-- ============================
-- 3. 批量SN最新状态查询
-- 用途：同时查询多个芯片的当前生产状态
-- 场景：客户查询、生产调度
-- 注意：最多支持10个SN同时查询
-- ============================
SELECT
    sn,                     -- 产品序列号
    MAX(op_code) AS current_op,  -- 当前所在工序
    MAX(create_time) AS last_time  -- 最后过站时间
FROM mes_work_order_sn
WHERE sn IN (:sn1, :sn2, :sn3)  -- SN列表
GROUP BY sn  -- 按SN分组
ORDER BY sn;  -- 按SN排序

-- 历史记录：
-- 2026-04-06：初始版本
-- 后续计划添加：按时间段查询过站记录