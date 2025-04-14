import {
  __commonJS
} from "./chunk-FOQIPI7F.js";

// node_modules/govuk-frontend/govuk/components/accordion/accordion.js
var require_accordion = __commonJS({
  "node_modules/govuk-frontend/govuk/components/accordion/accordion.js"(exports, module) {
    (function(global2, factory) {
      typeof exports === "object" && typeof module !== "undefined" ? module.exports = factory() : typeof define === "function" && define.amd ? define("GOVUKFrontend.Accordion", factory) : (global2.GOVUKFrontend = global2.GOVUKFrontend || {}, global2.GOVUKFrontend.Accordion = factory());
    })(exports, function() {
      "use strict";
      function nodeListForEach(nodes, callback) {
        if (window.NodeList.prototype.forEach) {
          return nodes.forEach(callback);
        }
        for (var i = 0; i < nodes.length; i++) {
          callback.call(window, nodes[i], i, nodes);
        }
      }
      (function(undefined) {
        var detect = (
          // In IE8, defineProperty could only act on DOM elements, so full support
          // for the feature requires the ability to set a property on an arbitrary object
          "defineProperty" in Object && function() {
            try {
              var a = {};
              Object.defineProperty(a, "test", { value: 42 });
              return true;
            } catch (e) {
              return false;
            }
          }()
        );
        if (detect) return;
        (function(nativeDefineProperty) {
          var supportsAccessors = Object.prototype.hasOwnProperty("__defineGetter__");
          var ERR_ACCESSORS_NOT_SUPPORTED = "Getters & setters cannot be defined on this javascript engine";
          var ERR_VALUE_ACCESSORS = "A property cannot both have accessors and be writable or have a value";
          Object.defineProperty = function defineProperty(object, property, descriptor) {
            if (nativeDefineProperty && (object === window || object === document || object === Element.prototype || object instanceof Element)) {
              return nativeDefineProperty(object, property, descriptor);
            }
            if (object === null || !(object instanceof Object || typeof object === "object")) {
              throw new TypeError("Object.defineProperty called on non-object");
            }
            if (!(descriptor instanceof Object)) {
              throw new TypeError("Property description must be an object");
            }
            var propertyString = String(property);
            var hasValueOrWritable = "value" in descriptor || "writable" in descriptor;
            var getterType = "get" in descriptor && typeof descriptor.get;
            var setterType = "set" in descriptor && typeof descriptor.set;
            if (getterType) {
              if (getterType !== "function") {
                throw new TypeError("Getter must be a function");
              }
              if (!supportsAccessors) {
                throw new TypeError(ERR_ACCESSORS_NOT_SUPPORTED);
              }
              if (hasValueOrWritable) {
                throw new TypeError(ERR_VALUE_ACCESSORS);
              }
              Object.__defineGetter__.call(object, propertyString, descriptor.get);
            } else {
              object[propertyString] = descriptor.value;
            }
            if (setterType) {
              if (setterType !== "function") {
                throw new TypeError("Setter must be a function");
              }
              if (!supportsAccessors) {
                throw new TypeError(ERR_ACCESSORS_NOT_SUPPORTED);
              }
              if (hasValueOrWritable) {
                throw new TypeError(ERR_VALUE_ACCESSORS);
              }
              Object.__defineSetter__.call(object, propertyString, descriptor.set);
            }
            if ("value" in descriptor) {
              object[propertyString] = descriptor.value;
            }
            return object;
          };
        })(Object.defineProperty);
      }).call("object" === typeof window && window || "object" === typeof self && self || "object" === typeof global && global || {});
      (function(undefined) {
        var detect = "bind" in Function.prototype;
        if (detect) return;
        Object.defineProperty(Function.prototype, "bind", {
          value: function bind(that) {
            var $Array = Array;
            var $Object = Object;
            var ObjectPrototype = $Object.prototype;
            var ArrayPrototype = $Array.prototype;
            var Empty = function Empty2() {
            };
            var to_string = ObjectPrototype.toString;
            var hasToStringTag = typeof Symbol === "function" && typeof Symbol.toStringTag === "symbol";
            var isCallable;
            var fnToStr = Function.prototype.toString, tryFunctionObject = function tryFunctionObject2(value) {
              try {
                fnToStr.call(value);
                return true;
              } catch (e) {
                return false;
              }
            }, fnClass = "[object Function]", genClass = "[object GeneratorFunction]";
            isCallable = function isCallable2(value) {
              if (typeof value !== "function") {
                return false;
              }
              if (hasToStringTag) {
                return tryFunctionObject(value);
              }
              var strClass = to_string.call(value);
              return strClass === fnClass || strClass === genClass;
            };
            var array_slice = ArrayPrototype.slice;
            var array_concat = ArrayPrototype.concat;
            var array_push = ArrayPrototype.push;
            var max = Math.max;
            var target = this;
            if (!isCallable(target)) {
              throw new TypeError("Function.prototype.bind called on incompatible " + target);
            }
            var args = array_slice.call(arguments, 1);
            var bound;
            var binder = function() {
              if (this instanceof bound) {
                var result = target.apply(
                  this,
                  array_concat.call(args, array_slice.call(arguments))
                );
                if ($Object(result) === result) {
                  return result;
                }
                return this;
              } else {
                return target.apply(
                  that,
                  array_concat.call(args, array_slice.call(arguments))
                );
              }
            };
            var boundLength = max(0, target.length - args.length);
            var boundArgs = [];
            for (var i = 0; i < boundLength; i++) {
              array_push.call(boundArgs, "$" + i);
            }
            bound = Function("binder", "return function (" + boundArgs.join(",") + "){ return binder.apply(this, arguments); }")(binder);
            if (target.prototype) {
              Empty.prototype = target.prototype;
              bound.prototype = new Empty();
              Empty.prototype = null;
            }
            return bound;
          }
        });
      }).call("object" === typeof window && window || "object" === typeof self && self || "object" === typeof global && global || {});
      (function(undefined) {
        var detect = "DOMTokenList" in this && function(x) {
          return "classList" in x ? !x.classList.toggle("x", false) && !x.className : true;
        }(document.createElement("x"));
        if (detect) return;
        (function(global2) {
          var nativeImpl = "DOMTokenList" in global2 && global2.DOMTokenList;
          if (!nativeImpl || !!document.createElementNS && !!document.createElementNS("http://www.w3.org/2000/svg", "svg") && !(document.createElementNS("http://www.w3.org/2000/svg", "svg").classList instanceof DOMTokenList)) {
            global2.DOMTokenList = function() {
              var dpSupport = true;
              var defineGetter = function(object, name, fn, configurable) {
                if (Object.defineProperty)
                  Object.defineProperty(object, name, {
                    configurable: false === dpSupport ? true : !!configurable,
                    get: fn
                  });
                else object.__defineGetter__(name, fn);
              };
              try {
                defineGetter({}, "support");
              } catch (e) {
                dpSupport = false;
              }
              var _DOMTokenList = function(el, prop) {
                var that = this;
                var tokens = [];
                var tokenMap = {};
                var length = 0;
                var maxLength = 0;
                var addIndexGetter = function(i) {
                  defineGetter(that, i, function() {
                    preop();
                    return tokens[i];
                  }, false);
                };
                var reindex = function() {
                  if (length >= maxLength)
                    for (; maxLength < length; ++maxLength) {
                      addIndexGetter(maxLength);
                    }
                };
                var preop = function() {
                  var error;
                  var i;
                  var args = arguments;
                  var rSpace = /\s+/;
                  if (args.length) {
                    for (i = 0; i < args.length; ++i)
                      if (rSpace.test(args[i])) {
                        error = new SyntaxError('String "' + args[i] + '" contains an invalid character');
                        error.code = 5;
                        error.name = "InvalidCharacterError";
                        throw error;
                      }
                  }
                  if (typeof el[prop] === "object") {
                    tokens = ("" + el[prop].baseVal).replace(/^\s+|\s+$/g, "").split(rSpace);
                  } else {
                    tokens = ("" + el[prop]).replace(/^\s+|\s+$/g, "").split(rSpace);
                  }
                  if ("" === tokens[0]) tokens = [];
                  tokenMap = {};
                  for (i = 0; i < tokens.length; ++i)
                    tokenMap[tokens[i]] = true;
                  length = tokens.length;
                  reindex();
                };
                preop();
                defineGetter(that, "length", function() {
                  preop();
                  return length;
                });
                that.toLocaleString = that.toString = function() {
                  preop();
                  return tokens.join(" ");
                };
                that.item = function(idx) {
                  preop();
                  return tokens[idx];
                };
                that.contains = function(token) {
                  preop();
                  return !!tokenMap[token];
                };
                that.add = function() {
                  preop.apply(that, args = arguments);
                  for (var args, token, i = 0, l = args.length; i < l; ++i) {
                    token = args[i];
                    if (!tokenMap[token]) {
                      tokens.push(token);
                      tokenMap[token] = true;
                    }
                  }
                  if (length !== tokens.length) {
                    length = tokens.length >>> 0;
                    if (typeof el[prop] === "object") {
                      el[prop].baseVal = tokens.join(" ");
                    } else {
                      el[prop] = tokens.join(" ");
                    }
                    reindex();
                  }
                };
                that.remove = function() {
                  preop.apply(that, args = arguments);
                  for (var args, ignore = {}, i = 0, t = []; i < args.length; ++i) {
                    ignore[args[i]] = true;
                    delete tokenMap[args[i]];
                  }
                  for (i = 0; i < tokens.length; ++i)
                    if (!ignore[tokens[i]]) t.push(tokens[i]);
                  tokens = t;
                  length = t.length >>> 0;
                  if (typeof el[prop] === "object") {
                    el[prop].baseVal = tokens.join(" ");
                  } else {
                    el[prop] = tokens.join(" ");
                  }
                  reindex();
                };
                that.toggle = function(token, force) {
                  preop.apply(that, [token]);
                  if (undefined !== force) {
                    if (force) {
                      that.add(token);
                      return true;
                    } else {
                      that.remove(token);
                      return false;
                    }
                  }
                  if (tokenMap[token]) {
                    that.remove(token);
                    return false;
                  }
                  that.add(token);
                  return true;
                };
                return that;
              };
              return _DOMTokenList;
            }();
          }
          (function() {
            var e = document.createElement("span");
            if (!("classList" in e)) return;
            e.classList.toggle("x", false);
            if (!e.classList.contains("x")) return;
            e.classList.constructor.prototype.toggle = function toggle(token) {
              var force = arguments[1];
              if (force === undefined) {
                var add = !this.contains(token);
                this[add ? "add" : "remove"](token);
                return add;
              }
              force = !!force;
              this[force ? "add" : "remove"](token);
              return force;
            };
          })();
          (function() {
            var e = document.createElement("span");
            if (!("classList" in e)) return;
            e.classList.add("a", "b");
            if (e.classList.contains("b")) return;
            var native = e.classList.constructor.prototype.add;
            e.classList.constructor.prototype.add = function() {
              var args = arguments;
              var l = arguments.length;
              for (var i = 0; i < l; i++) {
                native.call(this, args[i]);
              }
            };
          })();
          (function() {
            var e = document.createElement("span");
            if (!("classList" in e)) return;
            e.classList.add("a");
            e.classList.add("b");
            e.classList.remove("a", "b");
            if (!e.classList.contains("b")) return;
            var native = e.classList.constructor.prototype.remove;
            e.classList.constructor.prototype.remove = function() {
              var args = arguments;
              var l = arguments.length;
              for (var i = 0; i < l; i++) {
                native.call(this, args[i]);
              }
            };
          })();
        })(this);
      }).call("object" === typeof window && window || "object" === typeof self && self || "object" === typeof global && global || {});
      (function(undefined) {
        var detect = "Document" in this;
        if (detect) return;
        if (typeof WorkerGlobalScope === "undefined" && typeof importScripts !== "function") {
          if (this.HTMLDocument) {
            this.Document = this.HTMLDocument;
          } else {
            this.Document = this.HTMLDocument = document.constructor = new Function("return function Document() {}")();
            this.Document.prototype = document;
          }
        }
      }).call("object" === typeof window && window || "object" === typeof self && self || "object" === typeof global && global || {});
      (function(undefined) {
        var detect = "Element" in this && "HTMLElement" in this;
        if (detect) return;
        (function() {
          if (window.Element && !window.HTMLElement) {
            window.HTMLElement = window.Element;
            return;
          }
          window.Element = window.HTMLElement = new Function("return function Element() {}")();
          var vbody = document.appendChild(document.createElement("body"));
          var frame = vbody.appendChild(document.createElement("iframe"));
          var frameDocument = frame.contentWindow.document;
          var prototype = Element.prototype = frameDocument.appendChild(frameDocument.createElement("*"));
          var cache = {};
          var shiv = function(element, deep) {
            var childNodes = element.childNodes || [], index = -1, key, value, childNode;
            if (element.nodeType === 1 && element.constructor !== Element) {
              element.constructor = Element;
              for (key in cache) {
                value = cache[key];
                element[key] = value;
              }
            }
            while (childNode = deep && childNodes[++index]) {
              shiv(childNode, deep);
            }
            return element;
          };
          var elements = document.getElementsByTagName("*");
          var nativeCreateElement = document.createElement;
          var interval;
          var loopLimit = 100;
          prototype.attachEvent("onpropertychange", function(event) {
            var propertyName = event.propertyName, nonValue = !cache.hasOwnProperty(propertyName), newValue = prototype[propertyName], oldValue = cache[propertyName], index = -1, element;
            while (element = elements[++index]) {
              if (element.nodeType === 1) {
                if (nonValue || element[propertyName] === oldValue) {
                  element[propertyName] = newValue;
                }
              }
            }
            cache[propertyName] = newValue;
          });
          prototype.constructor = Element;
          if (!prototype.hasAttribute) {
            prototype.hasAttribute = function hasAttribute(name) {
              return this.getAttribute(name) !== null;
            };
          }
          function bodyCheck() {
            if (!loopLimit--) clearTimeout(interval);
            if (document.body && !document.body.prototype && /(complete|interactive)/.test(document.readyState)) {
              shiv(document, true);
              if (interval && document.body.prototype) clearTimeout(interval);
              return !!document.body.prototype;
            }
            return false;
          }
          if (!bodyCheck()) {
            document.onreadystatechange = bodyCheck;
            interval = setInterval(bodyCheck, 25);
          }
          document.createElement = function createElement(nodeName) {
            var element = nativeCreateElement(String(nodeName).toLowerCase());
            return shiv(element);
          };
          document.removeChild(vbody);
        })();
      }).call("object" === typeof window && window || "object" === typeof self && self || "object" === typeof global && global || {});
      (function(undefined) {
        var detect = "document" in this && "classList" in document.documentElement && "Element" in this && "classList" in Element.prototype && function() {
          var e = document.createElement("span");
          e.classList.add("a", "b");
          return e.classList.contains("b");
        }();
        if (detect) return;
        (function(global2) {
          var dpSupport = true;
          var defineGetter = function(object, name, fn, configurable) {
            if (Object.defineProperty)
              Object.defineProperty(object, name, {
                configurable: false === dpSupport ? true : !!configurable,
                get: fn
              });
            else object.__defineGetter__(name, fn);
          };
          try {
            defineGetter({}, "support");
          } catch (e) {
            dpSupport = false;
          }
          var addProp = function(o, name, attr) {
            defineGetter(o.prototype, name, function() {
              var tokenList;
              var THIS = this, gibberishProperty = "__defineGetter__DEFINE_PROPERTY" + name;
              if (THIS[gibberishProperty]) return tokenList;
              THIS[gibberishProperty] = true;
              if (false === dpSupport) {
                var visage;
                var mirror = addProp.mirror || document.createElement("div");
                var reflections = mirror.childNodes;
                var l = reflections.length;
                for (var i = 0; i < l; ++i)
                  if (reflections[i]._R === THIS) {
                    visage = reflections[i];
                    break;
                  }
                visage || (visage = mirror.appendChild(document.createElement("div")));
                tokenList = DOMTokenList.call(visage, THIS, attr);
              } else tokenList = new DOMTokenList(THIS, attr);
              defineGetter(THIS, name, function() {
                return tokenList;
              });
              delete THIS[gibberishProperty];
              return tokenList;
            }, true);
          };
          addProp(global2.Element, "classList", "className");
          addProp(global2.HTMLElement, "classList", "className");
          addProp(global2.HTMLLinkElement, "relList", "rel");
          addProp(global2.HTMLAnchorElement, "relList", "rel");
          addProp(global2.HTMLAreaElement, "relList", "rel");
        })(this);
      }).call("object" === typeof window && window || "object" === typeof self && self || "object" === typeof global && global || {});
      function Accordion($module) {
        this.$module = $module;
        this.moduleId = $module.getAttribute("id");
        this.$sections = $module.querySelectorAll(".govuk-accordion__section");
        this.$showAllButton = "";
        this.browserSupportsSessionStorage = helper.checkForSessionStorage();
        this.controlsClass = "govuk-accordion__controls";
        this.showAllClass = "govuk-accordion__show-all";
        this.showAllTextClass = "govuk-accordion__show-all-text";
        this.sectionExpandedClass = "govuk-accordion__section--expanded";
        this.sectionButtonClass = "govuk-accordion__section-button";
        this.sectionHeaderClass = "govuk-accordion__section-header";
        this.sectionHeadingClass = "govuk-accordion__section-heading";
        this.sectionHeadingTextClass = "govuk-accordion__section-heading-text";
        this.sectionHeadingTextFocusClass = "govuk-accordion__section-heading-text-focus";
        this.sectionShowHideToggleClass = "govuk-accordion__section-toggle";
        this.sectionShowHideToggleFocusClass = "govuk-accordion__section-toggle-focus";
        this.sectionShowHideTextClass = "govuk-accordion__section-toggle-text";
        this.upChevronIconClass = "govuk-accordion-nav__chevron";
        this.downChevronIconClass = "govuk-accordion-nav__chevron--down";
        this.sectionSummaryClass = "govuk-accordion__section-summary";
        this.sectionSummaryFocusClass = "govuk-accordion__section-summary-focus";
      }
      Accordion.prototype.init = function() {
        if (!this.$module) {
          return;
        }
        this.initControls();
        this.initSectionHeaders();
        var areAllSectionsOpen = this.checkIfAllSectionsOpen();
        this.updateShowAllButton(areAllSectionsOpen);
      };
      Accordion.prototype.initControls = function() {
        this.$showAllButton = document.createElement("button");
        this.$showAllButton.setAttribute("type", "button");
        this.$showAllButton.setAttribute("class", this.showAllClass);
        this.$showAllButton.setAttribute("aria-expanded", "false");
        var $icon = document.createElement("span");
        $icon.classList.add(this.upChevronIconClass);
        this.$showAllButton.appendChild($icon);
        var $accordionControls = document.createElement("div");
        $accordionControls.setAttribute("class", this.controlsClass);
        $accordionControls.appendChild(this.$showAllButton);
        this.$module.insertBefore($accordionControls, this.$module.firstChild);
        var $wrappershowAllText = document.createElement("span");
        $wrappershowAllText.classList.add(this.showAllTextClass);
        this.$showAllButton.appendChild($wrappershowAllText);
        this.$showAllButton.addEventListener("click", this.onShowOrHideAllToggle.bind(this));
      };
      Accordion.prototype.initSectionHeaders = function() {
        nodeListForEach(this.$sections, (function($section, i) {
          var $header = $section.querySelector("." + this.sectionHeaderClass);
          this.constructHeaderMarkup($header, i);
          this.setExpanded(this.isExpanded($section), $section);
          $header.addEventListener("click", this.onSectionToggle.bind(this, $section));
          this.setInitialState($section);
        }).bind(this));
      };
      Accordion.prototype.constructHeaderMarkup = function($headerWrapper, index) {
        var $span = $headerWrapper.querySelector("." + this.sectionButtonClass);
        var $heading = $headerWrapper.querySelector("." + this.sectionHeadingClass);
        var $summary = $headerWrapper.querySelector("." + this.sectionSummaryClass);
        var $button = document.createElement("button");
        $button.setAttribute("type", "button");
        $button.setAttribute("aria-controls", this.moduleId + "-content-" + (index + 1));
        for (var i = 0; i < $span.attributes.length; i++) {
          var attr = $span.attributes.item(i);
          if (attr.nodeName !== "id") {
            $button.setAttribute(attr.nodeName, attr.nodeValue);
          }
        }
        var $headingText = document.createElement("span");
        $headingText.classList.add(this.sectionHeadingTextClass);
        $headingText.id = $span.id;
        var $headingTextFocus = document.createElement("span");
        $headingTextFocus.classList.add(this.sectionHeadingTextFocusClass);
        $headingText.appendChild($headingTextFocus);
        $headingTextFocus.innerHTML = $span.innerHTML;
        var $showToggle = document.createElement("span");
        $showToggle.classList.add(this.sectionShowHideToggleClass);
        $showToggle.setAttribute("data-nosnippet", "");
        var $showToggleFocus = document.createElement("span");
        $showToggleFocus.classList.add(this.sectionShowHideToggleFocusClass);
        $showToggle.appendChild($showToggleFocus);
        var $showToggleText = document.createElement("span");
        var $icon = document.createElement("span");
        $icon.classList.add(this.upChevronIconClass);
        $showToggleFocus.appendChild($icon);
        $showToggleText.classList.add(this.sectionShowHideTextClass);
        $showToggleFocus.appendChild($showToggleText);
        $button.appendChild($headingText);
        $button.appendChild(this.getButtonPunctuationEl());
        if (typeof $summary !== "undefined" && $summary !== null) {
          var $summarySpan = document.createElement("span");
          var $summarySpanFocus = document.createElement("span");
          $summarySpanFocus.classList.add(this.sectionSummaryFocusClass);
          $summarySpan.appendChild($summarySpanFocus);
          for (var j = 0, l = $summary.attributes.length; j < l; ++j) {
            var nodeName = $summary.attributes.item(j).nodeName;
            var nodeValue = $summary.attributes.item(j).nodeValue;
            $summarySpan.setAttribute(nodeName, nodeValue);
          }
          $summarySpanFocus.innerHTML = $summary.innerHTML;
          $summary.parentNode.replaceChild($summarySpan, $summary);
          $button.appendChild($summarySpan);
          $button.appendChild(this.getButtonPunctuationEl());
        }
        $button.appendChild($showToggle);
        $heading.removeChild($span);
        $heading.appendChild($button);
      };
      Accordion.prototype.onSectionToggle = function($section) {
        var expanded = this.isExpanded($section);
        this.setExpanded(!expanded, $section);
        this.storeState($section);
      };
      Accordion.prototype.onShowOrHideAllToggle = function() {
        var $module = this;
        var $sections = this.$sections;
        var nowExpanded = !this.checkIfAllSectionsOpen();
        nodeListForEach($sections, function($section) {
          $module.setExpanded(nowExpanded, $section);
          $module.storeState($section);
        });
        $module.updateShowAllButton(nowExpanded);
      };
      Accordion.prototype.setExpanded = function(expanded, $section) {
        var $icon = $section.querySelector("." + this.upChevronIconClass);
        var $showHideText = $section.querySelector("." + this.sectionShowHideTextClass);
        var $button = $section.querySelector("." + this.sectionButtonClass);
        var $newButtonText = expanded ? "Hide" : "Show";
        var $visuallyHiddenText = document.createElement("span");
        $visuallyHiddenText.classList.add("govuk-visually-hidden");
        $visuallyHiddenText.innerHTML = " this section";
        $showHideText.innerHTML = $newButtonText;
        $showHideText.appendChild($visuallyHiddenText);
        $button.setAttribute("aria-expanded", expanded);
        if (expanded) {
          $section.classList.add(this.sectionExpandedClass);
          $icon.classList.remove(this.downChevronIconClass);
        } else {
          $section.classList.remove(this.sectionExpandedClass);
          $icon.classList.add(this.downChevronIconClass);
        }
        var areAllSectionsOpen = this.checkIfAllSectionsOpen();
        this.updateShowAllButton(areAllSectionsOpen);
      };
      Accordion.prototype.isExpanded = function($section) {
        return $section.classList.contains(this.sectionExpandedClass);
      };
      Accordion.prototype.checkIfAllSectionsOpen = function() {
        var sectionsCount = this.$sections.length;
        var expandedSectionCount = this.$module.querySelectorAll("." + this.sectionExpandedClass).length;
        var areAllSectionsOpen = sectionsCount === expandedSectionCount;
        return areAllSectionsOpen;
      };
      Accordion.prototype.updateShowAllButton = function(expanded) {
        var $showAllIcon = this.$showAllButton.querySelector("." + this.upChevronIconClass);
        var $showAllText = this.$showAllButton.querySelector("." + this.showAllTextClass);
        var newButtonText = expanded ? "Hide all sections" : "Show all sections";
        this.$showAllButton.setAttribute("aria-expanded", expanded);
        $showAllText.innerHTML = newButtonText;
        if (expanded) {
          $showAllIcon.classList.remove(this.downChevronIconClass);
        } else {
          $showAllIcon.classList.add(this.downChevronIconClass);
        }
      };
      var helper = {
        checkForSessionStorage: function() {
          var testString = "this is the test string";
          var result;
          try {
            window.sessionStorage.setItem(testString, testString);
            result = window.sessionStorage.getItem(testString) === testString.toString();
            window.sessionStorage.removeItem(testString);
            return result;
          } catch (exception) {
            if (typeof console === "undefined" || typeof console.log === "undefined") {
              console.log("Notice: sessionStorage not available.");
            }
          }
        }
      };
      Accordion.prototype.storeState = function($section) {
        if (this.browserSupportsSessionStorage) {
          var $button = $section.querySelector("." + this.sectionButtonClass);
          if ($button) {
            var contentId = $button.getAttribute("aria-controls");
            var contentState = $button.getAttribute("aria-expanded");
            if (typeof contentId === "undefined" && (typeof console === "undefined" || typeof console.log === "undefined")) {
              console.error(new Error("No aria controls present in accordion section heading."));
            }
            if (typeof contentState === "undefined" && (typeof console === "undefined" || typeof console.log === "undefined")) {
              console.error(new Error("No aria expanded present in accordion section heading."));
            }
            if (contentId && contentState) {
              window.sessionStorage.setItem(contentId, contentState);
            }
          }
        }
      };
      Accordion.prototype.setInitialState = function($section) {
        if (this.browserSupportsSessionStorage) {
          var $button = $section.querySelector("." + this.sectionButtonClass);
          if ($button) {
            var contentId = $button.getAttribute("aria-controls");
            var contentState = contentId ? window.sessionStorage.getItem(contentId) : null;
            if (contentState !== null) {
              this.setExpanded(contentState === "true", $section);
            }
          }
        }
      };
      Accordion.prototype.getButtonPunctuationEl = function() {
        var $punctuationEl = document.createElement("span");
        $punctuationEl.classList.add("govuk-visually-hidden", "govuk-accordion__section-heading-divider");
        $punctuationEl.innerHTML = ", ";
        return $punctuationEl;
      };
      return Accordion;
    });
  }
});
export default require_accordion();
//# sourceMappingURL=accordion-6DYGRLTA.js.map
