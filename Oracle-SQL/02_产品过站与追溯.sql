-- ===============================================
-- 产品过站与追溯相关SQL脚本
-- 功能：SN过站履历查询、未过站SN查询、批量SN状态查询
-- 优化：添加注释、参数化查询、性能优化
-- ===============================================

-- ------------------------------------------------
-- 根据SN查询完整过站履历
-- 功能：查询指定SN的完整生产过站记录
-- 参数：:sn - 产品序列号
-- 性能优化：建议在sn字段上创建索引，在op_code字段上创建索引
-- ------------------------------------------------
SELECT
    wos.sn,               -- 产品序列号
    wos.work_order_no,    -- 工单号
    wos.op_code,          -- 工序代码
    op.op_name,           -- 工序名称
    wos.equip_code,       -- 设备代码
    wos.check_result,     -- 检查结果
    wos.create_time       -- 过站时间
FROM mes_work_order_sn wos
LEFT JOIN mes_operation op
    ON wos.op_code = op.op_code  -- 关联工序表获取工序名称
WHERE wos.sn = :sn
ORDER BY wos.create_time ASC;  -- 按过站时间升序排列

-- ------------------------------------------------
-- 查询工单某工序未过站SN
-- 功能：查询指定工单指定工序中未完成过站的SN
-- 参数：
--   :work_order_no - 工单号
--   :op_code - 工序代码
-- 性能优化：建议在(work_order_no, op_code, check_result)上创建复合索引
-- ------------------------------------------------
SELECT
    sn,               -- 产品序列号
    work_order_no,    -- 工单号
    op_code,          -- 工序代码
    create_time       -- 创建时间
FROM mes_work_order_sn
WHERE work_order_no = :work_order_no
  AND op_code = :op_code
  AND check_result IS NULL  -- 未过站（检查结果为空）
ORDER BY create_time ASC;   -- 按创建时间升序排列

-- ------------------------------------------------
-- 批量多SN最新工序状态
-- 功能：查询多个SN的最新工序状态
-- 参数：:sn1, :sn2, :sn3 - 产品序列号
-- 性能优化：建议在sn字段上创建索引，在(op_code, create_time)上创建复合索引
-- 注意：实际使用时，可根据需要扩展SN数量
-- ------------------------------------------------
SELECT
    sn,                     -- 产品序列号
    MAX(op_code) AS current_op,  -- 最新工序代码
    MAX(create_time) AS last_time  -- 最新过站时间
FROM mes_work_order_sn
WHERE sn IN (:sn1, :sn2, :sn3)  -- 批量查询多个SN
GROUP BY sn  -- 按SN分组
ORDER BY sn;  -- 按SN排序