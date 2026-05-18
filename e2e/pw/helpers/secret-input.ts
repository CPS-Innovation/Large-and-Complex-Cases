import { Locator } from "@playwright/test";

export async function fillSecretInput(
  locator: Locator,
  value: string
) {
  await locator.evaluate((el, v) => {
    const input = el as HTMLInputElement;

    const nativeInputValueSetter =
      Object.getOwnPropertyDescriptor(
        window.HTMLInputElement.prototype,
        "value"
      )?.set;

    nativeInputValueSetter?.call(input, v);

    input.dispatchEvent(
      new Event("input", { bubbles: true })
    );
  }, value);
}