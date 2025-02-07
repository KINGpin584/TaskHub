import { combineReducers } from 'redux';
import menuReducer from './navbarreducers';

const rootReducer = combineReducers({
    menu: menuReducer,
    // other reducers can be added here
});

export default rootReducer;