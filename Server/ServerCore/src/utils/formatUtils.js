import fs from "fs";
import path from "path";
import camelCase from "lodash/camelCase.js";
import { config } from "../config/config";

export class FormatUtils {
  /**---------------------------------------------
   * snake_case -> camelCase 변환
   * @param {any} obj - 변환할 객체 또는 배열
   * @returns {any} - camelCase로 변환된 객체 또는 배열
   ---------------------------------------------*/
  static toCamelCase(obj) {
    if (Array.isArray(obj)) {
      return obj.map((v) => Utils.toCamelCase(v));
    } else if (obj !== null && typeof obj === "object" && obj.constructor === Object) {
      return Object.keys(obj).reduce((result, key) => {
        result[camelCase(key)] = Utils.toCamelCase(obj[key]);
        return result;
      }, {});
    }
    return obj;
  }

  /**---------------------------------------------
   * Date -> 문자열 변환
   * @param {Date} date - 변환할 날짜 객체
   * @returns {string} - 포맷된 날짜 문자열
   ---------------------------------------------*/
  static formatDate(date) {
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, "0");
    const day = String(date.getDate()).padStart(2, "0");
    const hours = String(date.getHours()).padStart(2, "0");
    const minutes = String(date.getMinutes()).padStart(2, "0");
    const seconds = String(date.getSeconds()).padStart(2, "0");

    return `${year}-${month}-${day} ${hours}:${minutes}:${seconds}`;
  }
}
