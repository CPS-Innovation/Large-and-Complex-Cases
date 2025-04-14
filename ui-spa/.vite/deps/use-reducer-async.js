import {
  require_react
} from "./chunk-XSOSCO6V.js";
import {
  __toESM
} from "./chunk-FOQIPI7F.js";

// node_modules/use-reducer-async/dist/index.modern.js
var import_react = __toESM(require_react());
var i = "undefined" == typeof window || /ServerSideRendering/.test(window.navigator && window.navigator.userAgent) ? import_react.useEffect : import_react.useLayoutEffect;
function u(e2, u2, a, s) {
  const d = (() => {
    const [t2, r2] = (0, import_react.useState)(() => new AbortController()), e3 = (0, import_react.useRef)(t2);
    return (0, import_react.useEffect)(() => () => {
      e3.current.abort(), e3.current = new AbortController(), r2(e3.current);
    }, []), t2.signal;
  })(), l = s || a, [w, g] = (0, import_react.useReducer)(e2, u2, s && a), p = (0, import_react.useRef)(w);
  i(() => {
    p.current = w;
  }, [w]);
  const f = (0, import_react.useCallback)(() => p.current, []), b = (0, import_react.useCallback)((t2) => {
    const { type: n2 } = t2 || {}, r2 = n2 && l[n2] || null;
    r2 ? r2({ dispatch: b, getState: f, signal: d })(t2) : g(t2);
  }, [l, f, d]);
  return [w, b];
}
export {
  u as useReducerAsync
};
//# sourceMappingURL=use-reducer-async.js.map
