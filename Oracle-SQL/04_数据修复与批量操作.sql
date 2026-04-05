-- 批量重置工序过站状态
UPDATE mes_work_order_sn
SET
    check_result = NULL,
    update_time = SYSDATE,
    update_user = :user
WHERE work_order_no = :work_order_no
  AND op_code = :op_code;

COMMIT;

-- 删除指定SN错误过站记录
DELETE FROM mes_work_order_sn
WHERE sn = :sn
  AND op_code = :op_code;

COMMIT;

-- 批量更新工单物料使用数量
UPDATE mes_work_order_material
SET
    qty_used = :qty,
    update_time = SYSDATE
WHERE work_order_no = :work_order_no
  AND material_no = :material_no;

COMMIT;