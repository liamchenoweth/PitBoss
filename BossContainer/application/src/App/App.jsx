import React from 'react';
import AppBar from '@material-ui/core/AppBar';
import Toolbar from '@material-ui/core/Toolbar';
import IconButton from '@material-ui/core/IconButton';
import Typography from '@material-ui/core/Typography';
import Drawer from '@material-ui/core/Drawer';
import List from '@material-ui/core/List';
import ListItem from '@material-ui/core/ListItem';
import ListItemText from '@material-ui/core/ListItemText';
import ListItemIcon from '@material-ui/core/ListItemIcon';
import HomeIcon from '@material-ui/icons/Home';
import PipelineIcon from '@material-ui/icons/LinearScale';
import OperationsIcon from '@material-ui/icons/Memory';
import RequestsIcon from '@material-ui/icons/Input';
import CssBaseline from '@material-ui/core/CssBaseline';
import { makeStyles } from '@material-ui/core/styles';
import {
  BrowserRouter as Router,
  Switch,
  Route,
  Link,
  useRouteMatch
} from "react-router-dom";

// Pages
import Pipelines from '../Pipelines/Pipelines.jsx';
import Operations from '../Operations/Operations';
import Requests from '../Requests/Requests';

const drawerWidth = 240;

const styles = makeStyles(theme => ({
  root: {
    display: 'flex',
  },
  drawer: {
    width: drawerWidth,
    flexShrink: 0,
  },
  appBar: {
    zIndex: theme.zIndex.drawer + 1,
  },
  paper: {
    width: drawerWidth,
    marginTop: 64
  },
  mainWindow: {
    width: "100%",
    padding: "2.5%",
    marginTop: 64,
    overflow: "hidden"
  },
  link: {
    textDecoration: "none",
    color: "unset"
  },
  selectedLink: {
    backgroundColor: "rgba(0, 0, 0, 0.15) !important"
  }
}));

function MenuItem(props) {
  let classes = styles();
  var match = useRouteMatch(props.link);
  var isSelected = match && (props.link == "/" ? match.isExact : true);
  console.log(isSelected)
  return (
    <Link to={props.link} className={classes.link}>
      <ListItem button selected={isSelected} classes={{ selected: classes.selectedLink }}>
        <ListItemIcon>
          {props.icon}
        </ListItemIcon>
        <ListItemText primary={props.name}/>
      </ListItem>
    </Link>
  )
}

function Menu() {
  return(
    <div>
      <List>
        <MenuItem name="Home" icon={<HomeIcon/>} link="/"/>
        <MenuItem name="Pipelines" icon={<PipelineIcon/>} link="/pipelines"/>
        <MenuItem name="Operations" icon={<OperationsIcon/>} link="/operations"/>
        <MenuItem name="Requests" icon={<RequestsIcon/>} link="/requests"/>
      </List>
    </div>
  )
}

function App() {
  var classes = styles();
  return (
    <div className="App" className={classes.root}>
      <Router>
        <CssBaseline />
        <AppBar position="fixed" className={classes.appBar}>
          <Toolbar>
            <Typography variant="h6">
              PitBoss
            </Typography>
          </Toolbar>
        </AppBar>
        <Drawer className={classes.drawer} classes={{ paper: classes.paper}} variant="permanent">
          <Menu/>
        </Drawer>
        <main className={classes.mainWindow}>
          <Switch>
            <Route exact path="/">
              Home
            </Route>
            <Route path="/pipelines">
              <Pipelines/>
            </Route>
            <Route path="/operations">
              <Operations/>
            </Route>
            <Route path="/requests">
              <Requests/>
            </Route>
            <Route>Fallback (404 Not Found)</Route>
          </Switch>
        </main>
      </Router>
    </div>
  );
}

export default App;
