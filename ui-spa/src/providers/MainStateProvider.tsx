import { ReactNode, useMemo, createContext, useContext } from "react";
import {
  mainStateReducer,
  initialState,
  MainStateActions,
  MainState,
} from "../reducers/mainStateReducer";
import { useReducerAsync } from "use-reducer-async";

interface MainStateProviderProps {
  children: ReactNode;
}

interface MainStateContextProps {
  state: MainState;
  dispatch: React.Dispatch<MainStateActions>;
}

const MainStateContext = createContext<MainStateContextProps | undefined>(
  undefined,
);

export const MainStateProvider: React.FC<MainStateProviderProps> = (props) => {
  const [state, dispatch] = useReducerAsync(mainStateReducer, initialState, {});
  const memoisedState = useMemo(() => {
    return state;
  }, [state]);

  return (
    <MainStateContext.Provider value={{ state: memoisedState, dispatch }}>
      {props.children}
    </MainStateContext.Provider>
  );
};

export const useMainStateContext = () => {
  return useContext(MainStateContext);
};
