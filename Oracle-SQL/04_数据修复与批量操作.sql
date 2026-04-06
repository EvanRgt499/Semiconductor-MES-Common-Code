-- ===============================================
-- 数据修复与批量操作相关SQL脚本
-- 功能：批量重置工序过站状态、删除错误过站记录、批量更新物料使用数量
-- 优化：添加注释、参数化查询、性能优化
-- ===============================================

-- ------------------------------------------------
-- 批量重置工序过站状态
-- 功能：将指定工单指定工序的所有SN过站状态重置为未过站
-- 参数：
--   :work_order_no - 工单号
--   :op_code - 工序代码
--   :user - 操作人
-- 性能优化：建议在(work_order_no, op_code)上创建复合索引
-- ------------------------------------------------
UPDATE mes_work_order_sn
SET
    check_result = NULL,       -- 重置为未过站状态
    update_time = SYSDATE,     -- 更新时间为当前系统时间
    update_user = :user        -- 操作人
WHERE work_order_no = :work_order_no
  AND op_code = :op_code;

-- 提交事务
COMMIT;

-- ------------------------------------------------
-- 删除指定SN错误过站记录
-- 功能：删除指定SN在指定工序的错误过站记录
-- 参数：
--   :sn - 产品序列号
--   :op_code - 工序代码
-- 性能优化：建议在(sn, op_code)上创建复合索引
-- ------------------------------------------------
DELETE FROM mes_work_order_sn
WHERE sn = :sn
  AND op_code = :op_code;

-- 提交事务
COMMIT;

-- ------------------------------------------------
-- 批量更新工单物料使用数量
-- 功能：更新指定工单指定物料的使用数量
-- 参数：
--   :work_order_no - 工单号
--   :material_no - 物料号
--   :qty - 使用数量
-- 性能优化：建议在(work_order_no, material_no)上创建复合索引
-- ------------------------------------------------
UPDATE mes_work_order_material
SET
    qty_used = :qty,          -- 更新使用数量
    update_time = SYSDATE      -- 更新时间为当前系统时间
WHERE work_order_no = :work_order_no
  AND material_no = :material_no;

-- 提交事务
COMMIT;