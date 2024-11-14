export class CustomError extends Error {
    /**---------------------------------------------
     * @param {number} code - 오류 코드
     * @param {string} message - 오류 메시지
     ---------------------------------------------*/
    constructor(code, message) {
      super(message);
      this.code = code;
      this.name = "CustomError";
    }
  }
  