-- 半导体封装MES系统 - 数据修复与批量操作SQL脚本
-- 作者：[孟斯辰]
-- 日期：2026-04-06
-- 说明：用于生产数据修复和批量操作

-- ============================
-- 1. 批量重置工序过站状态
-- 用途：当某工序出现批量不良时，重置所有芯片的过站状态
-- 场景：设备异常导致的批量数据问题
-- 注意：此操作会影响生产记录，需谨慎使用
-- ============================
UPDATE mes_work_order_sn
SET
    check_result = NULL,       -- 重置为未过站状态
    update_time = SYSDATE,     -- 更新时间戳
    update_user = :user        -- 操作人
WHERE work_order_no = :work_order_no  -- 工单号
  AND op_code = :op_code;  -- 工序代码

-- 提交事务
COMMIT;

-- ============================
-- 2. 删除错误过站记录
-- 用途：删除指定芯片在某工序的错误过站记录
-- 场景：操作员误操作、设备数据错误
-- ============================
DELETE FROM mes_work_order_sn
WHERE sn = :sn  -- 产品序列号
  AND op_code = :op_code;  -- 工序代码

-- 提交事务
COMMIT;

-- ============================
-- 3. 批量更新物料使用数量
-- 用途：调整工单物料实际使用量
-- 场景：物料盘点后调整、BOM变更
-- ============================
UPDATE mes_work_order_material
SET
    qty_used = :qty,          -- 更新使用数量
    update_time = SYSDATE      -- 更新时间戳
WHERE work_order_no = :work_order_no  -- 工单号
  AND material_no = :material_no;  -- 物料号

-- 提交事务
COMMIT;

-- 历史记录：
-- 2026-04-06：初始版本
-- 后续计划添加：批量导入数据、批量更新工单状态