import { ReactNode, useMemo, createContext, useContext } from "react";
import {
  mainStateReducer,
  initialState,
  MainStateActions,
  MainState,
} from "../reducers/mainStateReducer";
import { useReducerAsync } from "use-reducer-async";
import {
  AsyncActions,
  asyncActionHandlers,
} from "../reducers/asyncActionHandlers";

interface MainStateProviderProps {
  children: ReactNode;
}

interface MainStateContextProps {
  state: MainState;
  dispatch: React.Dispatch<MainStateActions | AsyncActions>;
}

const MainStateContext = createContext<MainStateContextProps | undefined>(
  undefined,
);

export const MainStateProvider: React.FC<MainStateProviderProps> = (props) => {
  const [state, dispatch]: [
    MainState,
    React.Dispatch<MainStateActions | AsyncActions>,
  ] = useReducerAsync(mainStateReducer, initialState, asyncActionHandlers);
  const memoisedState = useMemo(() => {
    return state;
  }, [state]);

  return (
    <MainStateContext.Provider value={{ state: memoisedState, dispatch }}>
      {props.children}
    </MainStateContext.Provider>
  );
};

// eslint-disable-next-line react-refresh/only-export-components
export const useMainStateContext = () => {
  return useContext(MainStateContext);
};
