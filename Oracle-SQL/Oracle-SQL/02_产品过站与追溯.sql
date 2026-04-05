-- 根据SN查询完整过站履历
SELECT
    wos.sn,
    wos.work_order_no,
    wos.op_code,
    op.op_name,
    wos.equip_code,
    wos.check_result,
    wos.create_time
FROM mes_work_order_sn wos
LEFT JOIN mes_operation op
    ON wos.op_code = op.op_code
WHERE wos.sn = :sn
ORDER BY wos.create_time ASC;

-- 查询工单某工序未过站SN
SELECT
    sn,
    work_order_no,
    op_code,
    create_time
FROM mes_work_order_sn
WHERE work_order_no = :work_order_no
  AND op_code = :op_code
  AND check_result IS NULL;

-- 批量多SN最新工序状态
SELECT
    sn,
    MAX(op_code) AS current_op,
    MAX(create_time) AS last_time
FROM mes_work_order_sn
WHERE sn IN (:sn1, :sn2, :sn3)
GROUP BY sn;