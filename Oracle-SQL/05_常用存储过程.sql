CREATE OR REPLACE PROCEDURE sp_reset_sn_operation(
    p_sn            IN VARCHAR2,
    p_op_code       IN VARCHAR2,
    p_user          IN VARCHAR2,
    p_result        OUT VARCHAR2
) AS
BEGIN
    -- 重置过站结果
    UPDATE mes_work_order_sn
    SET check_result = NULL,
        update_time = SYSDATE,
        update_user = p_user
    WHERE sn = p_sn
      AND op_code = p_op_code;

    -- 删除对应不良记录
    DELETE FROM mes_sn_fail
    WHERE sn = p_sn
      AND op_code = p_op_code;

    p_result := 'SUCCESS';
    COMMIT;

EXCEPTION
    WHEN OTHERS THEN
        p_result := 'ERROR: ' || SQLERRM;
        ROLLBACK;
END;
/