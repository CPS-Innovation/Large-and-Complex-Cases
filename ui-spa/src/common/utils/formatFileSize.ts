const ONE_KB = 1000;
const ONE_MB = 1000 * ONE_KB;
const ONE_GB = 1000 * ONE_MB;

/**
 * This conversion is based on decimal system not binary
 */

export const formatFileSize = (bytes: number) => {
  const format = (value: number, unit: string): string => {
    //truncate to two decimal places without rounding
    const fixed = Math.trunc(value * 100) / 100;
    //removing trailing zeros after decimal places
    const trimmed = fixed.toString().replace(/\.0+$/, "");
    return `${trimmed} ${unit}`;
  };

  if (bytes < ONE_MB) return format(bytes / ONE_KB, "KB");
  if (bytes < ONE_GB) return format(bytes / ONE_MB, "MB");
  return format(bytes / ONE_GB, "GB");
};
