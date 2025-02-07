import { atom } from 'recoil';

// localStorage Effect Handler
const localStorageEffect = key => ({ setSelf, onSet }) => {
  // Load initial value from localStorage
  const savedValue = localStorage.getItem(key);
  if (savedValue != null) {
    setSelf(JSON.parse(savedValue));
  }

  // Subscribe to state changes
  onSet((newValue, _, isReset) => {
    isReset 
      ? localStorage.removeItem(key)
      : localStorage.setItem(key, JSON.stringify(newValue));
  });
};

// User State Atom
export const userState = atom({
  key: 'userState',
  default: null,
  effects: [localStorageEffect('userState')]
});