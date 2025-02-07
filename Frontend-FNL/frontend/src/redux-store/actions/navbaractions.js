export const TOGGLE_MENU = 'TOGGLE_MENU';
export const CLOSE_MENU = 'CLOSE_MENU';
export const toggleMenu = () => {
    return {
        type: TOGGLE_MENU,
    };
};

export const closeMenu = () => {
    return {
        type: CLOSE_MENU,
    };
};