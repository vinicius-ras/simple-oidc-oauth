import { cleanup, render } from '@testing-library/react';
import React from 'react';
import { Provider } from 'react-redux';
import { BrowserRouter as Router } from 'react-router-dom';
import NavigationBar from '../components/NavigationBar';
import AuthServerClaimTypes from '../data/AuthServerClaimTypes';
import createAppStore from '../redux/AppStoreCreation';
import userInfoSlice, { SerializableClaim, UserInfoData } from '../redux/slices/userInfoSlice';


afterEach(() => { cleanup() });


/** Retrieves an object containing a sample {@link UserInfoData} to be used in the tests.
 * @param claims
 *     An array containing representations of the claims to be used to construct the user.
 *     If not given, the default is for the user to have no associated claims.
 * @returns The built sample {@link UserInfoData} for the tests.
 */
function getSampleUserInfoData(claims: AuthServerClaimTypes[] = []): UserInfoData
{
  const serializableClaims = claims.map<SerializableClaim>(claim => ({ type: claim as string, value: "true" }));
  return {
    id: "076a8a3e-b4c1-4603-a34b-d6c0ba870351",
    name: "John Doe 6adca6d235d9432e862caf881a52a8d6",
    email: "john-doe-6adca6d235d9432e862caf881a52a8d6@email.com",
    claims: serializableClaims,
  }
}


test('renders the NavigationBar', () => {
  const appStore = createAppStore();
  render(
    <Provider store={appStore}>
      <Router>
        <NavigationBar />
      </Router>
    </Provider>
  );
});


test('NavigationBar expand/collapse button IS NOT displayed if user DID NOT sign-in', () => {
  // Arrange
  const appStore = createAppStore();

  // Act
  const renderResult = render(
    <Provider store={appStore}>
      <Router>
        <NavigationBar />
      </Router>
    </Provider>
  );

  // Assert
  const navigationMenu = renderResult.queryAllByTitle("Menu");
  expect(navigationMenu.length).toBe(0);
});


test('Expand/collapse button IS displayed if user DID sign-in', () => {
  // Arrange
  const appStore = createAppStore();
  const loggedInUserInfo = getSampleUserInfoData();

  // Act
  appStore.dispatch(userInfoSlice.actions.setUserInfo(loggedInUserInfo));
  var renderResult = render(
    <Provider store={appStore}>
      <Router>
        <NavigationBar />
      </Router>
    </Provider>
  );

  // Assert
  const expandNavButton = renderResult.getByTitle("Menu");
  expect(expandNavButton).toBeInTheDocument();
});


test('User without claims can only see his/her username and logout button', () => {
  // Arrange
  const appStore = createAppStore();
  const loggedInUserInfo = getSampleUserInfoData();

  // Act
  appStore.dispatch(userInfoSlice.actions.setUserInfo(loggedInUserInfo));
  var renderResult = render(
    <Provider store={appStore}>
      <Router>
        <NavigationBar />
      </Router>
    </Provider>
  );
  const expandNavButton = renderResult.getByTitle("Menu");
  expandNavButton.click();

  // Assert
  expect(renderResult.getByText(loggedInUserInfo.name, {exact: false})).toBeInTheDocument();
  expect(renderResult.getByText(/Log out/i)).toBeInTheDocument();
});


describe('User only views page links for those pages he/she has the right claims to see', () => {
  const expectedPagesAndClaims = {
    "Clients": [
      [AuthServerClaimTypes.CanViewClients],
      [AuthServerClaimTypes.CanEditClients],
      [AuthServerClaimTypes.CanViewClients, AuthServerClaimTypes.CanEditClients],
    ],
    "Users": [
      [AuthServerClaimTypes.CanViewUsers],
      [AuthServerClaimTypes.CanEditUsers],
      [AuthServerClaimTypes.CanViewUsers, AuthServerClaimTypes.CanEditUsers],
    ],
    "Resources": [
      [AuthServerClaimTypes.CanViewResources],
      [AuthServerClaimTypes.CanEditResources],
      [AuthServerClaimTypes.CanViewResources, AuthServerClaimTypes.CanEditResources],
    ],
  };

  for (let expectedLinkName in expectedPagesAndClaims) {
    const otherLinkNames = Object.keys(expectedPagesAndClaims).filter(key => key !== expectedLinkName) as (keyof typeof expectedPagesAndClaims)[];
    for (let userClaims of expectedPagesAndClaims[expectedLinkName as keyof typeof expectedPagesAndClaims]) {
      test(`User with claim(s) [${userClaims.join(", ")}] sees the NavigationBar entry "${expectedLinkName}"`, () => {
        // Arrange
        const appStore = createAppStore();
        const loggedInUserInfo = getSampleUserInfoData(userClaims);

        // Act
        appStore.dispatch(userInfoSlice.actions.setUserInfo(loggedInUserInfo));
        var renderResult = render(
          <Provider store={appStore}>
            <Router>
              <NavigationBar />
            </Router>
          </Provider>
        );
        const expandNavButton = renderResult.getByTitle("Menu");
        expandNavButton.click();

        // Assert
        expect(renderResult.getByText(expectedLinkName, {exact: false})).toBeInTheDocument();
        for (let unexpectedLinkName of otherLinkNames)
          expect(renderResult.queryAllByText(unexpectedLinkName, {exact: false}).length).toBe(0);
      });
    }
  }
});


test(`User with Client-related and User-related claims sees both the "Clients" and the "Users" menu entries`, () => {
  // Arrange
  const appStore = createAppStore();
  const loggedInUserInfo = getSampleUserInfoData([AuthServerClaimTypes.CanEditClients, AuthServerClaimTypes.CanViewUsers]);

  // Act
  appStore.dispatch(userInfoSlice.actions.setUserInfo(loggedInUserInfo));
  var renderResult = render(
    <Provider store={appStore}>
      <Router>
        <NavigationBar />
      </Router>
    </Provider>
  );
  const expandNavButton = renderResult.getByTitle("Menu");
  expandNavButton.click();

  // Assert
  expect(renderResult.getByText("Clients", {exact: false})).toBeInTheDocument();
  expect(renderResult.getByText("Users", {exact: false})).toBeInTheDocument();
  expect(renderResult.queryAllByText("Resources", {exact: false}).length).toBe(0);
});